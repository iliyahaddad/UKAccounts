using UKAccounts.Application.DTOs;

namespace UKAccounts.Application.Interfaces;

public interface ICompanyService
{
    Task<CompanyDto> CreateAsync(CreateCompanyRequest request, CancellationToken cancellationToken = default);
    Task<CompanyDto?> GetAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompanyDto>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CompanyDto> UpdateAsync(Guid companyId, CreateCompanyRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<AccountingPeriodDto> CreateAccountingPeriodAsync(Guid companyId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AccountingPeriodDto>> GetAccountingPeriodsAsync(Guid companyId, CancellationToken cancellationToken = default);
}
