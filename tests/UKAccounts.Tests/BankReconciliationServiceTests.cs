using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;
using UKAccounts.Infrastructure.Persistence;
using UKAccounts.Infrastructure.Services;
using Xunit;

namespace UKAccounts.Tests;

public class BankReconciliationServiceTests
{
    private async Task<(AppDbContext context, BankTransactionService transactionService, BankReconciliationService reconciliationService)> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var transactionRepository = new Repository<BankTransaction>(context);
        var invoiceRepository = new Repository<Invoice>(context);
        var lineRepository = new Repository<InvoiceLine>(context);
        var billRepository = new Repository<Bill>(context);
        var billLineRepository = new Repository<BillLine>(context);

        var logger = NullLogger<BankReconciliationService>.Instance;

        var transactionService = new BankTransactionService(transactionRepository, context, logger);
        var invoiceService = new InvoiceService(invoiceRepository, lineRepository, context, logger);
        var billService = new BillService(billRepository, billLineRepository, context, logger);
        var expenseService = new ExpenseService(new Repository<Expense>(context), context, logger);

        var reconciliationService = new BankReconciliationService(
            transactionService,
            invoiceService,
            billService,
            expenseService,
            logger);

        return (context, transactionService, reconciliationService);
    }

    [Fact]
    public async Task ReconcileAsync_ShouldReconcileTransaction()
    {
        using var (context, transactionService, reconciliationService) = await CreateContextAsync();

        var transaction = await transactionService.CreateAsync(new CreateBankTransactionRequest
        {
            CompanyId = Guid.Empty,
            BankAccountId = Guid.Empty,
            TransactionDate = DateTime.Today,
            Description = "Payment from Customer",
            Amount = 100,
            Type = TransactionType.Credit,
            Balance = 1000
        });

        var result = await reconciliationService.ReconcileAsync(transaction.Id, Guid.Empty, "Invoice");

        Assert.True(result.Success);
        Assert.Equal("Transaction reconciled successfully.", result.Message);
    }

    [Fact]
    public async Task UnreconcileAsync_ShouldUnreconcileTransaction()
    {
        using var (context, transactionService, reconciliationService) = await CreateContextAsync();

        var transaction = await transactionService.CreateAsync(new CreateBankTransactionRequest
        {
            CompanyId = Guid.Empty,
            BankAccountId = Guid.Empty,
            TransactionDate = DateTime.Today,
            Description = "Payment from Customer",
            Amount = 100,
            Type = TransactionType.Credit,
            Balance = 1000
        });

        await reconciliationService.ReconcileAsync(transaction.Id, Guid.Empty, "Invoice");
        var result = await reconciliationService.UnreconcileAsync(transaction.Id);

        Assert.True(result.Success);
        Assert.Equal("Transaction unreconciled successfully.", result.Message);
    }
}
