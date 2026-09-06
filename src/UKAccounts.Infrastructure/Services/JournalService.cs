using Microsoft.Extensions.Logging;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;
using UKAccounts.Accounting;

namespace UKAccounts.Infrastructure.Services;

public class JournalService : IJournalService
{
    private readonly IRepository<Journal> _journalRepository;
    private readonly IRepository<JournalLine> _lineRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<JournalService> _logger;

    public JournalService(
        IRepository<Journal> journalRepository,
        IRepository<JournalLine> lineRepository,
        IUnitOfWork unitOfWork,
        ILogger<JournalService> logger)
    {
        _journalRepository = journalRepository;
        _lineRepository = lineRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<JournalDto> CreateAsync(CreateJournalRequest request, CancellationToken cancellationToken = default)
    {
        if (!DoubleEntryValidator.IsBalanced(request.Lines.Select(l => new JournalLine { Debit = l.Debit, Credit = l.Credit })))
        {
            throw new InvalidOperationException("Journal is not balanced. Total debits must equal total credits.");
        }

        var journal = new Journal
        {
            CompanyId = request.CompanyId,
            AccountingPeriodId = request.AccountingPeriodId,
            Date = request.Date,
            Reference = request.Reference,
            Description = request.Description,
            Source = request.Source,
            Status = JournalStatus.Draft
        };

        await _journalRepository.AddAsync(journal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var line in request.Lines)
        {
            var journalLine = new JournalLine
            {
                JournalId = journal.Id,
                AccountId = line.AccountId,
                Debit = line.Debit,
                Credit = line.Credit,
                Description = line.Description,
                TaxCode = line.TaxCode,
                DocumentReference = line.DocumentReference
            };

            await _lineRepository.AddAsync(journalLine, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Journal created {JournalId}", journal.Id);
        return await ToDtoAsync(journal);
    }

    public async Task<JournalDto?> GetAsync(Guid journalId, CancellationToken cancellationToken = default)
    {
        var journal = await _journalRepository.GetByIdAsync(journalId, Guid.Empty, cancellationToken);
        return journal == null ? null : await ToDtoAsync(journal);
    }

    public async Task<IReadOnlyList<JournalDto>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var journals = await _journalRepository.GetByCompanyAsync(companyId, cancellationToken);
        var result = new List<JournalDto>();

        foreach (var journal in journals)
        {
            result.Add(await ToDtoAsync(journal));
        }

        return result;
    }

    public async Task DeleteAsync(Guid journalId, CancellationToken cancellationToken = default)
    {
        var journal = await _journalRepository.GetByIdAsync(journalId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Journal not found.");

        if (journal.Status == JournalStatus.Posted)
        {
            throw new InvalidOperationException("Cannot delete a posted journal. Reverse it instead.");
        }

        await _journalRepository.DeleteAsync(journal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Journal deleted {JournalId}", journalId);
    }

    private async Task<JournalDto> ToDtoAsync(Journal journal)
    {
        var lines = await _lineRepository.GetByCompanyAsync(journal.CompanyId, cancellationToken: default);
        var journalLines = lines.Where(l => l.JournalId == journal.Id).ToList();

        return new JournalDto
        {
            Id = journal.Id,
            CompanyId = journal.CompanyId,
            AccountingPeriodId = journal.AccountingPeriodId,
            Date = journal.Date,
            Reference = journal.Reference,
            Description = journal.Description,
            Source = journal.Source,
            Status = journal.Status,
            PostedByUserId = journal.PostedByUserId,
            PostedAt = journal.PostedAt,
            ReversedByJournalId = journal.ReversedByJournalId,
            Lines = journalLines.Select(l => new JournalLineDto
            {
                Id = l.Id,
                JournalId = l.JournalId,
                AccountId = l.AccountId,
                AccountCode = string.Empty,
                AccountName = string.Empty,
                Debit = l.Debit,
                Credit = l.Credit,
                Description = l.Description,
                TaxCode = l.TaxCode,
                DocumentReference = l.DocumentReference
            }).ToList()
        };
    }
}
