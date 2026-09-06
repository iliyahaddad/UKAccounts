using UKAccounts.Application.DTOs;

namespace UKAccounts.Application.Interfaces;

public interface IBankAccountService
{
    Task<BankAccountDto> CreateAsync(CreateBankAccountRequest request, CancellationToken cancellationToken = default);
    Task<BankAccountDto?> GetAsync(Guid bankAccountId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BankAccountDto>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<BankAccountDto> UpdateAsync(Guid bankAccountId, CreateBankAccountRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid bankAccountId, CancellationToken cancellationToken = default);
}
