using UKAccounts.Application.DTOs;

namespace UKAccounts.Application.Interfaces;

public interface IFilingService
{
    Task<FilingDto> CreateAsync(CreateFilingRequest request, CancellationToken cancellationToken = default);
    Task<FilingDto?> GetAsync(Guid filingId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FilingDto>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<FilingDto> UpdateStatusAsync(Guid filingId, string status, string? response = null, CancellationToken cancellationToken = default);
    Task<FilingAttemptDto> AddAttemptAsync(Guid filingId, string status, string? response = null, Guid? userId = null, CancellationToken cancellationToken = default);
    Task<FilingDocumentDto> AddDocumentAsync(Guid filingId, CreateFilingDocumentRequest request, Stream content, CancellationToken cancellationToken = default);
}

public class CreateFilingRequest
{
    public Guid CompanyId { get; set; }
    public Guid AccountingPeriodId { get; set; }
    public string FilingType { get; set; } = string.Empty;
    public string? GeneratedFileHash { get; set; }
    public string? ValidationResult { get; set; }
    public Guid? UserId { get; set; }
}

public class CreateFilingDocumentRequest
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Hash { get; set; } = string.Empty;
    public DocumentType Type { get; set; }
}
