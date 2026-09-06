using UKAccounts.Application.DTOs;

namespace UKAccounts.Application.Interfaces;

public interface ISupplierService
{
    Task<SupplierDto> CreateAsync(CreateSupplierRequest request, CancellationToken cancellationToken = default);
    Task<SupplierDto?> GetAsync(Guid supplierId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SupplierDto>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<SupplierDto> UpdateAsync(Guid supplierId, CreateSupplierRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid supplierId, CancellationToken cancellationToken = default);
}
