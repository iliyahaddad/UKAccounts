using UKAccounts.Application.DTOs;

namespace UKAccounts.Application.Interfaces;

public interface IBankStatementService
{
    Task<BankStatementDto> CreateAsync(CreateBankStatementRequest request, CancellationToken cancellationToken = default);
    Task<BankStatementDto?> GetAsync(Guid statementId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BankStatementDto>> GetByBankAccountAsync(Guid bankAccountId, CancellationToken cancellationToken = default);
    Task<BankStatementDto> UpdateStatusAsync(Guid statementId, StatementStatus status, string? errorMessage = null, CancellationToken cancellationToken = default);
}
