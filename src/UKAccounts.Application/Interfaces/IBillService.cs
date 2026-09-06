using UKAccounts.Application.DTOs;

namespace UKAccounts.Application.Interfaces;

public interface IBillService
{
    Task<BillDto> CreateAsync(CreateBillRequest request, CancellationToken cancellationToken = default);
    Task<BillDto?> GetAsync(Guid billId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BillDto>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<BillDto> UpdateAsync(Guid billId, CreateBillRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid billId, CancellationToken cancellationToken = default);
    Task<BillDto> ApproveAsync(Guid billId, CancellationToken cancellationToken = default);
    Task<BillDto> MarkAsPaidAsync(Guid billId, CancellationToken cancellationToken = default);
}
