using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;
using UKAccounts.Infrastructure.Persistence;
using UKAccounts.Infrastructure.Services;
using UKAccounts.Accounting;
using Xunit;

namespace UKAccounts.Tests;

public class AccountingEngineTests
{
    private async Task<(AppDbContext context, ChartOfAccountsService chartService, JournalService journalService, AccountingEngine engine)> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var accountRepository = new Repository<Account>(context);
        var journalRepository = new Repository<Journal>(context);
        var lineRepository = new Repository<JournalLine>(context);
        var periodRepository = new Repository<AccountingPeriod>(context);

        var logger = NullLogger<AccountingEngine>.Instance;

        var chartService = new ChartOfAccountsService(accountRepository, context, logger);
        var journalService = new JournalService(journalRepository, lineRepository, context, logger);
        var engine = new AccountingEngine(journalRepository, lineRepository, context, logger);

        return (context, chartService, journalService, engine);
    }

    [Fact]
    public async Task PostJournal_ShouldPostBalancedJournal()
    {
        using var (context, chartService, journalService, engine) = await CreateContextAsync();

        var cashAccount = await chartService.CreateAsync(new CreateAccountRequest
        {
            CompanyId = Guid.Empty,
            Code = "1000",
            Name = "Cash",
            Category = AccountCategory.Assets,
            Type = AccountType.Asset
        });

        var salesAccount = await chartService.CreateAsync(new CreateAccountRequest
        {
            CompanyId = Guid.Empty,
            Code = "4000",
            Name = "Sales",
            Category = AccountCategory.Income,
            Type = AccountType.Income
        });

        var request = new CreateJournalRequest
        {
            CompanyId = Guid.Empty,
            AccountingPeriodId = Guid.Empty,
            Date = DateTime.Today,
            Reference = "INV-001",
            Description = "Test invoice",
            Source = JournalSource.Manual,
            Lines = new List<CreateJournalLineRequest>
            {
                new CreateJournalLineRequest { AccountId = cashAccount.Id, Debit = 100, Credit = 0, Description = "Cash received" },
                new CreateJournalLineRequest { AccountId = salesAccount.Id, Debit = 0, Credit = 100, Description = "Sales income" }
            }
        };

        var journal = await journalService.CreateAsync(request);
        var result = await engine.PostJournalAsync(journal.Id, Guid.NewGuid());

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task PostJournal_ShouldFailForUnbalancedJournal()
    {
        using var (context, chartService, journalService, engine) = await CreateContextAsync();

        var cashAccount = await chartService.CreateAsync(new CreateAccountRequest
        {
            CompanyId = Guid.Empty,
            Code = "1000",
            Name = "Cash",
            Category = AccountCategory.Assets,
            Type = AccountType.Asset
        });

        var salesAccount = await chartService.CreateAsync(new CreateAccountRequest
        {
            CompanyId = Guid.Empty,
            Code = "4000",
            Name = "Sales",
            Category = AccountCategory.Income,
            Type = AccountType.Income
        });

        var request = new CreateJournalRequest
        {
            CompanyId = Guid.Empty,
            AccountingPeriodId = Guid.Empty,
            Date = DateTime.Today,
            Reference = "INV-002",
            Description = "Unbalanced journal",
            Source = JournalSource.Manual,
            Lines = new List<CreateJournalLineRequest>
            {
                new CreateJournalLineRequest { AccountId = cashAccount.Id, Debit = 100, Credit = 0, Description = "Cash received" },
                new CreateJournalLineRequest { AccountId = salesAccount.Id, Debit = 0, Credit = 50, Description = "Sales income" }
            }
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => journalService.CreateAsync(request));
    }

    [Fact]
    public async Task ReverseJournal_ShouldCreateReversalEntry()
    {
        using var (context, chartService, journalService, engine) = await CreateContextAsync();

        var cashAccount = await chartService.CreateAsync(new CreateAccountRequest
        {
            CompanyId = Guid.Empty,
            Code = "1000",
            Name = "Cash",
            Category = AccountCategory.Assets,
            Type = AccountType.Asset
        });

        var salesAccount = await chartService.CreateAsync(new CreateAccountRequest
        {
            CompanyId = Guid.Empty,
            Code = "4000",
            Name = "Sales",
            Category = AccountCategory.Income,
            Type = AccountType.Income
        });

        var request = new CreateJournalRequest
        {
            CompanyId = Guid.Empty,
            AccountingPeriodId = Guid.Empty,
            Date = DateTime.Today,
            Reference = "INV-003",
            Description = "Test invoice",
            Source = JournalSource.Manual,
            Lines = new List<CreateJournalLineRequest>
            {
                new CreateJournalLineRequest { AccountId = cashAccount.Id, Debit = 100, Credit = 0, Description = "Cash received" },
                new CreateJournalLineRequest { AccountId = salesAccount.Id, Debit = 0, Credit = 100, Description = "Sales income" }
            }
        };

        var journal = await journalService.CreateAsync(request);
        await engine.PostJournalAsync(journal.Id, Guid.NewGuid());

        var reversalResult = await engine.ReverseJournalAsync(journal.Id, Guid.NewGuid());
        Assert.True(reversalResult.IsSuccess);
    }
}
