using UKAccounts.Application.DTOs;

namespace UKAccounts.Application.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceDto> CreateAsync(CreateInvoiceRequest request, CancellationToken cancellationToken = default);
    Task<InvoiceDto?> GetAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InvoiceDto>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<InvoiceDto> UpdateAsync(Guid invoiceId, CreateInvoiceRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<InvoiceDto> IssueAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<InvoiceDto> MarkAsPaidAsync(Guid invoiceId, CancellationToken cancellationToken = default);
}
