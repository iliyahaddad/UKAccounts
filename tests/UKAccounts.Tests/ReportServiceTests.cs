using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;
using UKAccounts.Infrastructure.Persistence;
using UKAccounts.Infrastructure.Services;
using UKAccounts.Reporting;
using Xunit;

namespace UKAccounts.Tests;

public class ReportServiceTests
{
    private async Task<(AppDbContext context, ReportService service)> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var accountRepository = new Repository<Account>(context);
        var journalRepository = new Repository<Journal>(context);
        var lineRepository = new Repository<JournalLine>(context);
        var invoiceRepository = new Repository<Invoice>(context);
        var billRepository = new Repository<Bill>(context);
        var bankTransactionRepository = new Repository<BankTransaction>(context);

        var logger = NullLogger<ReportService>.Instance;

        var service = new ReportService(
            accountRepository,
            journalRepository,
            lineRepository,
            invoiceRepository,
            billRepository,
            bankTransactionRepository,
            logger);

        return (context, service);
    }

    [Fact]
    public async Task GetProfitLossAsync_ShouldReturnReportWithSections()
    {
        using var (context, service) = await CreateContextAsync();

        var incomeAccount = new Account
        {
            CompanyId = Guid.Empty,
            Code = "4000",
            Name = "Sales",
            Category = AccountCategory.Income,
            Type = AccountType.Income
        };

        var expenseAccount = new Account
        {
            CompanyId = Guid.Empty,
            Code = "5000",
            Name = "Rent",
            Category = AccountCategory.OperatingExpenses,
            Type = AccountType.Expense
        };

        context.Accounts.AddRange(incomeAccount, expenseAccount);
        await context.SaveChangesAsync();

        var journal = new Journal
        {
            CompanyId = Guid.Empty,
            AccountingPeriodId = Guid.Empty,
            Date = new DateTime(2025, 6, 1),
            Reference = "J001",
            Description = "Test",
            Source = JournalSource.Manual,
            Status = JournalStatus.Posted
        };

        context.Journals.Add(journal);
        await context.SaveChangesAsync();

        context.JournalLines.AddRange(
            new JournalLine { JournalId = journal.Id, AccountId = incomeAccount.Id, Debit = 0, Credit = 1000 },
            new JournalLine { JournalId = journal.Id, AccountId = expenseAccount.Id, Debit = 500, Credit = 0 }
        );
        await context.SaveChangesAsync();

        var report = await service.GetProfitLossAsync(Guid.Empty, new DateTime(2025, 1, 1), new DateTime(2025, 12, 31));

        Assert.Equal(1000, report.TotalRevenue);
        Assert.Equal(500, report.TotalExpenses);
        Assert.Equal(500, report.NetProfit);
        Assert.Equal(2, report.Sections.Count);
    }

    [Fact]
    public async Task GetBalanceSheetAsync_ShouldReturnBalancedReport()
    {
        using var (context, service) = await CreateContextAsync();

        var assetAccount = new Account
        {
            CompanyId = Guid.Empty,
            Code = "1000",
            Name = "Cash",
            Category = AccountCategory.Assets,
            Type = AccountType.Asset
        };

        var liabilityAccount = new Account
        {
            CompanyId = Guid.Empty,
            Code = "2000",
            Name = "Creditors",
            Category = AccountCategory.Liabilities,
            Type = AccountType.Liability
        };

        context.Accounts.AddRange(assetAccount, liabilityAccount);
        await context.SaveChangesAsync();

        var journal = new Journal
        {
            CompanyId = Guid.Empty,
            AccountingPeriodId = Guid.Empty,
            Date = new DateTime(2025, 6, 1),
            Reference = "J002",
            Description = "Test",
            Source = JournalSource.Manual,
            Status = JournalStatus.Posted
        };

        context.Journals.Add(journal);
        await context.SaveChangesAsync();

        context.JournalLines.AddRange(
            new JournalLine { JournalId = journal.Id, AccountId = assetAccount.Id, Debit = 1000, Credit = 0 },
            new JournalLine { JournalId = journal.Id, AccountId = liabilityAccount.Id, Debit = 0, Credit = 1000 }
        );
        await context.SaveChangesAsync();

        var report = await service.GetBalanceSheetAsync(Guid.Empty, new DateTime(2025, 12, 31));

        Assert.True(report.IsBalanced);
        Assert.Equal(1000, report.TotalAssets);
        Assert.Equal(1000, report.TotalLiabilities);
    }

    [Fact]
    public async Task GetAgedReceivablesAsync_ShouldCategorizeInvoicesByAge()
    {
        using var (context, service) = await CreateContextAsync();

        var customer = new Customer
        {
            CompanyId = Guid.Empty,
            Name = "Test Customer"
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var invoice = new Invoice
        {
            CompanyId = Guid.Empty,
            CustomerId = customer.Id,
            InvoiceNumber = "INV-001",
            Date = DateTime.Today.AddDays(-40),
            DueDate = DateTime.Today.AddDays(-10),
            Status = InvoiceStatus.Issued,
            Total = 500
        };

        context.Invoices.Add(invoice);
        await context.SaveChangesAsync();

        var report = await service.GetAgedReceivablesAsync(Guid.Empty, DateTime.Today);

        Assert.Single(report.Lines);
        Assert.Equal(500, report.Lines[0].Days31to60);
        Assert.Equal(500, report.TotalOutstanding);
    }
}
