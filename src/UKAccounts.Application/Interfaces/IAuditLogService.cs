namespace UKAccounts.Application.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLogEntry>> GetByUserAsync(Guid userId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLogEntry>> GetByCompanyAsync(Guid companyId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
}

public class AuditLogEntry
{
    public Guid? UserId { get; set; }
    public Guid? CompanyId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
