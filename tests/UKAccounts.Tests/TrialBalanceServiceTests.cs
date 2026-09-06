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

public class TrialBalanceServiceTests
{
    private async Task<(AppDbContext context, TrialBalanceService service)> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var journalRepository = new Repository<Journal>(context);
        var lineRepository = new Repository<JournalLine>(context);
        var accountRepository = new Repository<Account>(context);

        var logger = NullLogger<TrialBalanceService>.Instance;

        var service = new TrialBalanceService(
            journalRepository,
            lineRepository,
            accountRepository,
            logger);

        return (context, service);
    }

    [Fact]
    public async Task GetTrialBalance_ShouldReturnBalancedTrialBalance()
    {
        using var (context, service) = await CreateContextAsync();

        var account = new Account
        {
            CompanyId = Guid.Empty,
            Code = "1000",
            Name = "Cash",
            Category = AccountCategory.Assets,
            Type = AccountType.Asset
        };

        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var journal = new Journal
        {
            CompanyId = Guid.Empty,
            AccountingPeriodId = Guid.Empty,
            Date = DateTime.Today,
            Reference = "J001",
            Description = "Test",
            Source = JournalSource.Manual,
            Status = JournalStatus.Posted
        };

        context.Journals.Add(journal);
        await context.SaveChangesAsync();

        context.JournalLines.Add(new JournalLine
        {
            JournalId = journal.Id,
            AccountId = account.Id,
            Debit = 100,
            Credit = 0,
            Description = "Test"
        });

        context.JournalLines.Add(new JournalLine
        {
            JournalId = journal.Id,
            AccountId = account.Id,
            Debit = 0,
            Credit = 100,
            Description = "Test"
        });

        await context.SaveChangesAsync();

        var trialBalance = await service.GetTrialBalanceAsync(Guid.Empty, DateTime.Today);

        Assert.Single(trialBalance);
        Assert.Equal(100, trialBalance[0].Debit);
        Assert.Equal(100, trialBalance[0].Credit);
        Assert.True(await service.IsBalancedAsync(Guid.Empty, DateTime.Today));
    }
}
