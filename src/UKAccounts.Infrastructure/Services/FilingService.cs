using Microsoft.Extensions.Logging;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;

namespace UKAccounts.Infrastructure.Services;

public class FilingService : IFilingService
{
    private readonly IRepository<Filing> _filingRepository;
    private readonly IRepository<FilingDocument> _filingDocumentRepository;
    private readonly IRepository<FilingAttempt> _filingAttemptRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FilingService> _logger;

    public FilingService(
        IRepository<Filing> filingRepository,
        IRepository<FilingDocument> filingDocumentRepository,
        IRepository<FilingAttempt> filingAttemptRepository,
        IUnitOfWork unitOfWork,
        ILogger<FilingService> logger)
    {
        _filingRepository = filingRepository;
        _filingDocumentRepository = filingDocumentRepository;
        _filingAttemptRepository = filingAttemptRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<FilingDto> CreateAsync(CreateFilingRequest request, CancellationToken cancellationToken = default)
    {
        var filing = new Filing
        {
            CompanyId = request.CompanyId,
            AccountingPeriodId = request.AccountingPeriodId,
            FilingType = request.FilingType,
            Status = "Draft",
            GeneratedFileHash = request.GeneratedFileHash,
            ValidationResult = request.ValidationResult,
            UserId = request.UserId
        };

        await _filingRepository.AddAsync(filing, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Filing created {FilingId}", filing.Id);
        return ToDto(filing);
    }

    public async Task<FilingDto?> GetAsync(Guid filingId, CancellationToken cancellationToken = default)
    {
        var filing = await _filingRepository.GetByIdAsync(filingId, Guid.Empty, cancellationToken);
        return filing == null ? null : ToDto(filing);
    }

    public async Task<IReadOnlyList<FilingDto>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var filings = await _filingRepository.GetByCompanyAsync(companyId, cancellationToken);
        return filings.Select(ToDto).ToList();
    }

    public async Task<FilingDto> UpdateStatusAsync(Guid filingId, string status, string? response = null, CancellationToken cancellationToken = default)
    {
        var filing = await _filingRepository.GetByIdAsync(filingId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Filing not found.");

        filing.Status = status;
        filing.Response = response;

        if (status == "Submitted" || status == "Accepted" || status == "Rejected")
        {
            filing.SubmissionDate = DateTimeOffset.UtcNow;
        }

        await _filingRepository.UpdateAsync(filing, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Filing {FilingId} status updated to {Status}", filingId, status);
        return ToDto(filing);
    }

    public async Task<FilingAttemptDto> AddAttemptAsync(Guid filingId, string status, string? response = null, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var filing = await _filingRepository.GetByIdAsync(filingId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Filing not found.");

        var attempt = new FilingAttempt
        {
            FilingId = filingId,
            Status = status,
            Response = response,
            UserId = userId,
            AttemptedAt = DateTimeOffset.UtcNow
        };

        await _filingAttemptRepository.AddAsync(attempt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Filing attempt added for filing {FilingId}", filingId);
        return ToAttemptDto(attempt);
    }

    public async Task<FilingDocumentDto> AddDocumentAsync(Guid filingId, CreateFilingDocumentRequest request, Stream content, CancellationToken cancellationToken = default)
    {
        var filing = await _filingRepository.GetByIdAsync(filingId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Filing not found.");

        var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_{request.FileName}");
        using var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write);
        await content.CopyToAsync(fileStream, cancellationToken);

        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(content, cancellationToken);
        var hash = Convert.ToBase64String(hashBytes);

        var document = new FilingDocument
        {
            FilingId = filingId,
            FileName = request.FileName,
            FilePath = tempPath,
            ContentType = request.ContentType,
            Size = request.Size,
            Hash = hash,
            Type = request.Type
        };

        await _filingDocumentRepository.AddAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Filing document added {DocumentId} to filing {FilingId}", document.Id, filingId);
        return ToDocumentDto(document);
    }

    private static FilingDto ToDto(Filing filing)
    {
        return new FilingDto
        {
            Id = filing.Id,
            CompanyId = filing.CompanyId,
            AccountingPeriodId = filing.AccountingPeriodId,
            FilingType = filing.FilingType,
            Status = filing.Status,
            GeneratedFileHash = filing.GeneratedFileHash,
            SubmissionDate = filing.SubmissionDate,
            SubmissionId = filing.SubmissionId,
            Response = filing.Response,
            CreatedAt = filing.CreatedAt,
            Documents = filing.Documents.Select(ToDocumentDto).ToList(),
            Attempts = filing.Attempts.Select(ToAttemptDto).ToList()
        };
    }

    private static FilingDocumentDto ToDocumentDto(FilingDocument document)
    {
        return new FilingDocumentDto
        {
            Id = document.Id,
            FilingId = document.FilingId,
            FileName = document.FileName,
            ContentType = document.ContentType,
            Size = document.Size,
            Hash = document.Hash,
            Type = document.Type
        };
    }

    private static FilingAttemptDto ToAttemptDto(FilingAttempt attempt)
    {
        return new FilingAttemptDto
        {
            Id = attempt.Id,
            FilingId = attempt.FilingId,
            AttemptedAt = attempt.AttemptedAt,
            Status = attempt.Status,
            Response = attempt.Response
        };
    }
}
