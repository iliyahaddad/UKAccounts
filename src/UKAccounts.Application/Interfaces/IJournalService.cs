using UKAccounts.Application.DTOs;

namespace UKAccounts.Application.Interfaces;

public interface IJournalService
{
    Task<JournalDto> CreateAsync(CreateJournalRequest request, CancellationToken cancellationToken = default);
    Task<JournalDto?> GetAsync(Guid journalId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JournalDto>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid journalId, CancellationToken cancellationToken = default);
}
