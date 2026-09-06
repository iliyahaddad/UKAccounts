using UKAccounts.Application.DTOs;

namespace UKAccounts.Application.Interfaces;

public interface IImportMappingService
{
    Task<ImportMappingDto> CreateAsync(CreateImportMappingRequest request, CancellationToken cancellationToken = default);
    Task<ImportMappingDto?> GetAsync(Guid mappingId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ImportMappingDto>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<ImportMappingDto> UpdateAsync(Guid mappingId, CreateImportMappingRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid mappingId, CancellationToken cancellationToken = default);
    Task<ImportMappingDto?> GetDefaultAsync(Guid companyId, string sourceFormat, CancellationToken cancellationToken = default);
}
