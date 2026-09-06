using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;
using UKAccounts.Infrastructure.Persistence;
using UKAccounts.UK;
using Xunit;

namespace UKAccounts.Tests;

public class MicroEntityAccountsGeneratorTests
{
    private async Task<(AppDbContext context, MicroEntityAccountsGenerator generator)> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var companyRepository = new Repository<Company>(context);
        var periodRepository = new Repository<AccountingPeriod>(context);
        var journalRepository = new Repository<Journal>(context);
        var accountRepository = new Repository<Account>(context);
        var invoiceRepository = new Repository<Invoice>(context);
        var bankTransactionRepository = new Repository<BankTransaction>(context);

        var logger = NullLogger<MicroEntityAccountsGenerator>.Instance;

        var generator = new MicroEntityAccountsGenerator(
            companyRepository,
            periodRepository,
            journalRepository,
            accountRepository,
            invoiceRepository,
            bankTransactionRepository,
            logger);

        return (context, generator);
    }

    [Fact]
    public async Task GenerateAsync_ShouldGenerateMicroEntityAccounts()
    {
        using var (context, generator) = await CreateContextAsync();

        var company = new Company
        {
            CompanyNumber = "12345678",
            CompanyName = "Test Micro Ltd",
            IsDormant = false,
            AccountingReferenceDate = new DateTime(2025, 12, 31)
        };

        context.Companies.Add(company);
        await context.SaveChangesAsync();

        var period = new AccountingPeriod
        {
            CompanyId = company.Id,
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 12, 31),
            IsCurrent = true
        };

        context.AccountingPeriods.Add(period);
        await context.SaveChangesAsync();

        var request = new AccountsProductionRequest
        {
            CompanyId = company.Id,
            AccountingPeriodId = period.Id,
            Regime = AccountsRegime.MicroEntity,
            PeriodStart = new DateTime(2025, 1, 1),
            PeriodEnd = new DateTime(2025, 12, 31)
        };

        var result = await generator.GenerateAsync(request);

        Assert.True(result.Success);
        Assert.NotNull(result.IxbrlPath);
        Assert.Contains("micro_entity_accounts_12345678_20251231.ixbrl", result.IxbrlPath);
    }
}
