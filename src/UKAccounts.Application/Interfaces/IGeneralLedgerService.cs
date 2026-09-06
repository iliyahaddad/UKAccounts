using UKAccounts.Application.DTOs;

namespace UKAccounts.Application.Interfaces;

public interface IGeneralLedgerService
{
    Task<IReadOnlyList<LedgerEntryDto>> GetByAccountAsync(Guid companyId, Guid accountId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LedgerEntryDto>> GetByCompanyAsync(Guid companyId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
}
