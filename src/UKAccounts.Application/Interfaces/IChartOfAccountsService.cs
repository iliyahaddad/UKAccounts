using UKAccounts.Application.DTOs;

namespace UKAccounts.Application.Interfaces;

public interface IChartOfAccountsService
{
    Task<AccountDto> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken = default);
    Task<AccountDto?> GetAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AccountDto>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<AccountDto> UpdateAsync(Guid accountId, CreateAccountRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task ReorderAsync(Guid companyId, List<Guid> orderedAccountIds, CancellationToken cancellationToken = default);
}
