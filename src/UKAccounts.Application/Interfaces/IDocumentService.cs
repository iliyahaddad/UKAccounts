using UKAccounts.Application.DTOs;

namespace UKAccounts.Application.Interfaces;

public interface IDocumentService
{
    Task<DocumentDto> UploadAsync(CreateDocumentRequest request, Stream content, CancellationToken cancellationToken = default);
    Task<DocumentDto?> GetAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentDto>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentDto>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(Guid documentId, CancellationToken cancellationToken = default);
}
