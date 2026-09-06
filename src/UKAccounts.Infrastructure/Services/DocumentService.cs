using Microsoft.Extensions.Logging;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;

namespace UKAccounts.Infrastructure.Services;

public class DocumentService : IDocumentService
{
    private readonly IRepository<Document> _documentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        IRepository<Document> documentRepository,
        IUnitOfWork unitOfWork,
        ILogger<DocumentService> logger)
    {
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DocumentDto> UploadAsync(CreateDocumentRequest request, Stream content, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine("Documents", $"{Guid.NewGuid()}_{request.FileName}");

        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await content.CopyToAsync(fileStream, cancellationToken);

        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(content, cancellationToken);
        var hash = Convert.ToBase64String(hashBytes);

        var document = new Document
        {
            CompanyId = request.CompanyId,
            FileName = request.FileName,
            FilePath = filePath,
            ContentType = request.ContentType,
            Size = request.Size,
            Hash = hash,
            RelatedEntityType = request.RelatedEntityType,
            RelatedEntityId = request.RelatedEntityId,
            Description = request.Description,
            UploadedByUserId = request.UploadedByUserId
        };

        await _documentRepository.AddAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document uploaded {DocumentId} {FileName}", document.Id, document.FileName);
        return ToDto(document);
    }

    public async Task<DocumentDto?> GetAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(documentId, Guid.Empty, cancellationToken);
        return document == null ? null : ToDto(document);
    }

    public async Task<IReadOnlyList<DocumentDto>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var documents = await _documentRepository.GetByCompanyAsync(companyId, cancellationToken);
        return documents.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<DocumentDto>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default)
    {
        var documents = await _documentRepository.GetByCompanyAsync(Guid.Empty, cancellationToken);
        return documents
            .Where(d => d.RelatedEntityType == entityType && d.RelatedEntityId == entityId)
            .Select(ToDto)
            .ToList();
    }

    public async Task DeleteAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(documentId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Document not found.");

        if (File.Exists(document.FilePath))
        {
            File.Delete(document.FilePath);
        }

        await _documentRepository.DeleteAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document deleted {DocumentId}", documentId);
    }

    public async Task<Stream> DownloadAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(documentId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Document not found.");

        if (!File.Exists(document.FilePath))
        {
            throw new FileNotFoundException("Document file not found.", document.FilePath);
        }

        return new FileStream(document.FilePath, FileMode.Open, FileAccess.Read);
    }

    private static DocumentDto ToDto(Document document)
    {
        return new DocumentDto
        {
            Id = document.Id,
            CompanyId = document.CompanyId,
            FileName = document.FileName,
            ContentType = document.ContentType,
            Size = document.Size,
            Hash = document.Hash,
            RelatedEntityType = document.RelatedEntityType,
            RelatedEntityId = document.RelatedEntityId,
            Description = document.Description,
            UploadedAt = document.UploadedAt
        };
    }
}
