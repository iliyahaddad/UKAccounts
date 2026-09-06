using UKAccounts.Application.DTOs;

namespace UKAccounts.Application.Interfaces;

public interface ICustomerService
{
    Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<CustomerDto?> GetAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerDto>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<CustomerDto> UpdateAsync(Guid customerId, CreateCustomerRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid customerId, CancellationToken cancellationToken = default);
}
