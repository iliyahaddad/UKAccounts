using Microsoft.Extensions.Logging;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;

namespace UKAccounts.Infrastructure.Services;

public class ImportMappingService : IImportMappingService
{
    private readonly IRepository<ImportMapping> _mappingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ImportMappingService> _logger;

    public ImportMappingService(
        IRepository<ImportMapping> mappingRepository,
        IUnitOfWork unitOfWork,
        ILogger<ImportMappingService> logger)
    {
        _mappingRepository = mappingRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ImportMappingDto> CreateAsync(CreateImportMappingRequest request, CancellationToken cancellationToken = default)
    {
        var mapping = new ImportMapping
        {
            CompanyId = request.CompanyId,
            Name = request.Name,
            SourceFormat = request.SourceFormat,
            IsDefault = request.IsDefault,
            ColumnMappings = request.ColumnMappings
        };

        await _mappingRepository.AddAsync(mapping, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Import mapping created {MappingId}", mapping.Id);
        return ToDto(mapping);
    }

    public async Task<ImportMappingDto?> GetAsync(Guid mappingId, CancellationToken cancellationToken = default)
    {
        var mapping = await _mappingRepository.GetByIdAsync(mappingId, Guid.Empty, cancellationToken);
        return mapping == null ? null : ToDto(mapping);
    }

    public async Task<IReadOnlyList<ImportMappingDto>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var mappings = await _mappingRepository.GetByCompanyAsync(companyId, cancellationToken);
        return mappings.Select(ToDto).ToList();
    }

    public async Task<ImportMappingDto> UpdateAsync(Guid mappingId, CreateImportMappingRequest request, CancellationToken cancellationToken = default)
    {
        var mapping = await _mappingRepository.GetByIdAsync(mappingId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Import mapping not found.");

        mapping.Name = request.Name;
        mapping.SourceFormat = request.SourceFormat;
        mapping.IsDefault = request.IsDefault;
        mapping.ColumnMappings = request.ColumnMappings;
        mapping.UpdatedAt = DateTimeOffset.UtcNow;

        await _mappingRepository.UpdateAsync(mapping, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Import mapping updated {MappingId}", mapping.Id);
        return ToDto(mapping);
    }

    public async Task DeleteAsync(Guid mappingId, CancellationToken cancellationToken = default)
    {
        var mapping = await _mappingRepository.GetByIdAsync(mappingId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Import mapping not found.");

        await _mappingRepository.DeleteAsync(mapping, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Import mapping deleted {MappingId}", mappingId);
    }

    public async Task<ImportMappingDto?> GetDefaultAsync(Guid companyId, string sourceFormat, CancellationToken cancellationToken = default)
    {
        var mappings = await _mappingRepository.GetByCompanyAsync(companyId, cancellationToken);
        return mappings
            .Where(m => m.SourceFormat == sourceFormat && m.IsDefault)
            .Select(ToDto)
            .FirstOrDefault();
    }

    private static ImportMappingDto ToDto(ImportMapping mapping)
    {
        return new ImportMappingDto
        {
            Id = mapping.Id,
            CompanyId = mapping.CompanyId,
            Name = mapping.Name,
            SourceFormat = mapping.SourceFormat,
            IsDefault = mapping.IsDefault,
            ColumnMappings = mapping.ColumnMappings,
            CreatedAt = mapping.CreatedAt,
            UpdatedAt = mapping.UpdatedAt
        };
    }
}
