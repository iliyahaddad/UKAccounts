using Microsoft.Extensions.Logging;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;
using UKAccounts.Accounting;

namespace UKAccounts.Infrastructure.Services;

public class TrialBalanceService : ITrialBalanceService
{
    private readonly IRepository<Journal> _journalRepository;
    private readonly IRepository<JournalLine> _lineRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly ILogger<TrialBalanceService> _logger;

    public TrialBalanceService(
        IRepository<Journal> journalRepository,
        IRepository<JournalLine> lineRepository,
        IRepository<Account> accountRepository,
        ILogger<TrialBalanceService> logger)
    {
        _journalRepository = journalRepository;
        _lineRepository = lineRepository;
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TrialBalanceDto>> GetTrialBalanceAsync(Guid companyId, DateTime to, CancellationToken cancellationToken = default)
    {
        var journals = await _journalRepository.GetByCompanyAsync(companyId, cancellationToken);
        var lines = await _lineRepository.GetByCompanyAsync(companyId, cancellationToken);
        var accounts = await _accountRepository.GetByCompanyAsync(companyId, cancellationToken);

        var postedJournals = journals
            .Where(j => j.CompanyId == companyId && j.Status == JournalStatus.Posted && j.Date <= to)
            .ToList();

        var postedLines = lines
            .Where(l => postedJournals.Select(j => j.Id).Contains(l.JournalId))
            .ToList();

        var accountDict = accounts.ToDictionary(a => a.Id);

        var trialBalance = postedLines
            .GroupBy(l => l.AccountId)
            .Select(g =>
            {
                var account = accountDict.TryGetValue(g.Key, out var acc) ? acc : null;
                return new TrialBalanceDto
                {
                    AccountId = g.Key,
                    AccountCode = account?.Code ?? string.Empty,
                    AccountName = account?.Name ?? "Unknown",
                    Category = account?.Category.ToString() ?? "Unknown",
                    Debit = g.Sum(l => l.Debit),
                    Credit = g.Sum(l => l.Credit)
                };
            })
            .OrderBy(t => t.AccountCode)
            .ToList();

        return trialBalance;
    }

    public async Task<bool> IsBalancedAsync(Guid companyId, DateTime to, CancellationToken cancellationToken = default)
    {
        var trialBalance = await GetTrialBalanceAsync(companyId, to, cancellationToken);
        var totalDebits = trialBalance.Sum(t => t.Debit);
        var totalCredits = trialBalance.Sum(t => t.Credit);
        return totalDebits == totalCredits;
    }
}
