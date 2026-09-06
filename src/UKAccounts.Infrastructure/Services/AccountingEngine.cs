using Microsoft.Extensions.Logging;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;
using UKAccounts.Accounting;

namespace UKAccounts.Infrastructure.Services;

public class AccountingEngine : IAccountingEngine
{
    private readonly IRepository<Journal> _journalRepository;
    private readonly IRepository<JournalLine> _lineRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AccountingEngine> _logger;

    public AccountingEngine(
        IRepository<Journal> journalRepository,
        IRepository<JournalLine> lineRepository,
        IUnitOfWork unitOfWork,
        ILogger<AccountingEngine> logger)
    {
        _journalRepository = journalRepository;
        _lineRepository = lineRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> PostJournalAsync(Guid journalId, Guid userId, CancellationToken cancellationToken = default)
    {
        var journal = await _journalRepository.GetByIdAsync(journalId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Journal not found.");

        if (journal.Status == JournalStatus.Posted)
        {
            return Result.Failure("Journal is already posted.");
        }

        if (journal.Status == JournalStatus.Reversed)
        {
            return Result.Failure("Cannot post a reversed journal.");
        }

        var lines = await _lineRepository.GetByCompanyAsync(journal.CompanyId, cancellationToken);
        var journalLines = lines.Where(l => l.JournalId == journalId).ToList();

        if (!DoubleEntryValidator.IsBalanced(journalLines))
        {
            var totalDebits = journalLines.Sum(l => l.Debit);
            var totalCredits = journalLines.Sum(l => l.Credit);
            _logger.LogWarning("Journal {JournalId} is not balanced. Debits: {Debits}, Credits: {Credits}", journalId, totalDebits, totalCredits);
            return Result.Failure($"Journal is not balanced. Total debits ({totalDebits}) must equal total credits ({totalCredits}).");
        }

        journal.Status = JournalStatus.Posted;
        journal.PostedByUserId = userId;
        journal.PostedAt = DateTimeOffset.UtcNow;

        await _journalRepository.UpdateAsync(journal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Journal posted {JournalId} by {UserId}", journalId, userId);
        return Result.Success();
    }

    public async Task<Result> ReverseJournalAsync(Guid journalId, Guid userId, CancellationToken cancellationToken = default)
    {
        var journal = await _journalRepository.GetByIdAsync(journalId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Journal not found.");

        if (journal.Status != JournalStatus.Posted)
        {
            return Result.Failure("Only posted journals can be reversed.");
        }

        if (journal.ReversedByJournalId.HasValue)
        {
            return Result.Failure("Journal has already been reversed.");
        }

        var lines = await _lineRepository.GetByCompanyAsync(journal.CompanyId, cancellationToken);
        var journalLines = lines.Where(l => l.JournalId == journalId).ToList();

        var reversalJournal = new Journal
        {
            CompanyId = journal.CompanyId,
            AccountingPeriodId = journal.AccountingPeriodId,
            Date = DateTime.UtcNow,
            Reference = $"REV-{journal.Reference}",
            Description = $"Reversal of {journal.Reference}: {journal.Description}",
            Source = JournalSource.System,
            Status = JournalStatus.Posted,
            PostedByUserId = userId,
            PostedAt = DateTimeOffset.UtcNow
        };

        await _journalRepository.AddAsync(reversalJournal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var line in journalLines)
        {
            var reversalLine = new JournalLine
            {
                JournalId = reversalJournal.Id,
                AccountId = line.AccountId,
                Debit = line.Credit,
                Credit = line.Debit,
                Description = $"Reversal of {line.Description}",
                TaxCode = line.TaxCode,
                DocumentReference = line.DocumentReference
            };

            await _lineRepository.AddAsync(reversalLine, cancellationToken);
        }

        journal.ReversedByJournalId = reversalJournal.Id;
        journal.Status = JournalStatus.Reversed;

        await _journalRepository.UpdateAsync(journal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Journal {JournalId} reversed by {UserId} with reversal journal {ReversalJournalId}", journalId, userId, reversalJournal.Id);
        return Result.Success();
    }
}
