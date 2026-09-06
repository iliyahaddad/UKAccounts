using UKAccounts.Application.DTOs;

namespace UKAccounts.Application.Interfaces;

public interface IBankTransactionService
{
    Task<BankTransactionDto> CreateAsync(CreateBankTransactionRequest request, CancellationToken cancellationToken = default);
    Task<BankTransactionDto?> GetAsync(Guid transactionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BankTransactionDto>> GetByBankAccountAsync(Guid bankAccountId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BankTransactionDto>> GetUnreconciledAsync(Guid bankAccountId, CancellationToken cancellationToken = default);
    Task<BankTransactionDto> ReconcileAsync(Guid transactionId, Guid matchedJournalId, CancellationToken cancellationToken = default);
    Task<BankTransactionDto> UnreconcileAsync(Guid transactionId, CancellationToken cancellationToken = default);
}
