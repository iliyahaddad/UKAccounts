using Microsoft.Extensions.Logging;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;

namespace UKAccounts.Infrastructure.Services;

public class GeneralLedgerService : IGeneralLedgerService
{
    private readonly IRepository<Journal> _journalRepository;
    private readonly IRepository<JournalLine> _lineRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly ILogger<GeneralLedgerService> _logger;

    public GeneralLedgerService(
        IRepository<Journal> journalRepository,
        IRepository<JournalLine> lineRepository,
        IRepository<Account> accountRepository,
        ILogger<GeneralLedgerService> logger)
    {
        _journalRepository = journalRepository;
        _lineRepository = lineRepository;
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<LedgerEntryDto>> GetByAccountAsync(Guid companyId, Guid accountId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var journals = await _journalRepository.GetByCompanyAsync(companyId, cancellationToken);
        var lines = await _lineRepository.GetByCompanyAsync(companyId, cancellationToken);
        var accounts = await _accountRepository.GetByCompanyAsync(companyId, cancellationToken);
        var accountDict = accounts.ToDictionary(a => a.Id);

        var journalList = journals
            .Where(j => j.CompanyId == companyId && j.Status == JournalStatus.Posted && j.Date >= from && j.Date <= to)
            .ToList();

        var result = new List<LedgerEntryDto>();

        foreach (var journal in journalList)
        {
            var journalLines = lines.Where(l => l.JournalId == journal.Id && l.AccountId == accountId).ToList();

            foreach (var line in journalLines)
            {
                result.Add(new LedgerEntryDto
                {
                    Id = line.Id,
                    JournalId = journal.Id,
                    JournalReference = journal.Reference,
                    JournalDate = journal.Date,
                    AccountId = line.AccountId,
                    AccountCode = accountDict.TryGetValue(line.AccountId, out var acc) ? acc.Code : string.Empty,
                    AccountName = accountDict.TryGetValue(line.AccountId, out acc) ? acc.Name : string.Empty,
                    Debit = line.Debit,
                    Credit = line.Credit,
                    Description = line.Description,
                    PostedAt = journal.PostedAt ?? DateTimeOffset.UtcNow
                });
            }
        }

        return result.OrderBy(r => r.JournalDate).ToList();
    }

    public async Task<IReadOnlyList<LedgerEntryDto>> GetByCompanyAsync(Guid companyId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var journals = await _journalRepository.GetByCompanyAsync(companyId, cancellationToken);
        var lines = await _lineRepository.GetByCompanyAsync(companyId, cancellationToken);
        var accounts = await _accountRepository.GetByCompanyAsync(companyId, cancellationToken);
        var accountDict = accounts.ToDictionary(a => a.Id);

        var journalList = journals
            .Where(j => j.CompanyId == companyId && j.Status == JournalStatus.Posted && j.Date >= from && j.Date <= to)
            .ToList();

        var result = new List<LedgerEntryDto>();

        foreach (var journal in journalList)
        {
            var journalLines = lines.Where(l => l.JournalId == journal.Id).ToList();

            foreach (var line in journalLines)
            {
                result.Add(new LedgerEntryDto
                {
                    Id = line.Id,
                    JournalId = journal.Id,
                    JournalReference = journal.Reference,
                    JournalDate = journal.Date,
                    AccountId = line.AccountId,
                    AccountCode = accountDict.TryGetValue(line.AccountId, out var acc) ? acc.Code : string.Empty,
                    AccountName = accountDict.TryGetValue(line.AccountId, out acc) ? acc.Name : string.Empty,
                    Debit = line.Debit,
                    Credit = line.Credit,
                    Description = line.Description,
                    PostedAt = journal.PostedAt ?? DateTimeOffset.UtcNow
                });
            }
        }

        return result.OrderBy(r => r.JournalDate).ToList();
    }
}
