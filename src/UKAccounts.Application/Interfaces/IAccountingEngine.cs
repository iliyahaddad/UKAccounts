namespace UKAccounts.Application.Interfaces;

public interface IAccountingEngine
{
    Task<Result> PostJournalAsync(Guid journalId, Guid userId, CancellationToken cancellationToken = default);
    Task<Result> ReverseJournalAsync(Guid journalId, Guid userId, CancellationToken cancellationToken = default);
}
