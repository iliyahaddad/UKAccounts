using Microsoft.Extensions.Logging;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;

namespace UKAccounts.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IRepository<AuditLog> _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(
        IRepository<AuditLog> auditLogRepository,
        IUnitOfWork unitOfWork,
        ILogger<AuditLogService> logger)
    {
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task LogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            UserId = entry.UserId,
            CompanyId = entry.CompanyId,
            Action = entry.Action,
            EntityType = entry.EntityType,
            EntityId = entry.EntityId,
            OldValue = entry.OldValue,
            NewValue = entry.NewValue,
            IpAddress = entry.IpAddress,
            UserAgent = entry.UserAgent,
            Timestamp = DateTimeOffset.UtcNow
        };

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Audit log: {Action} {EntityType} {EntityId} by {UserId}", entry.Action, entry.EntityType, entry.EntityId, entry.UserId);
    }

    public async Task<IReadOnlyList<AuditLogEntry>> GetByUserAsync(Guid userId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var logs = await _auditLogRepository.GetByCompanyAsync(Guid.Empty, cancellationToken);
        var query = logs.Where(a => a.UserId == userId);

        if (from.HasValue) query = query.Where(a => a.Timestamp >= from.Value);
        if (to.HasValue) query = query.Where(a => a.Timestamp <= to.Value);

        return query.OrderByDescending(a => a.Timestamp)
            .Select(a => new AuditLogEntry
            {
                UserId = a.UserId,
                CompanyId = a.CompanyId,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                OldValue = a.OldValue,
                NewValue = a.NewValue,
                IpAddress = a.IpAddress,
                UserAgent = a.UserAgent
            })
            .ToList();
    }

    public async Task<IReadOnlyList<AuditLogEntry>> GetByCompanyAsync(Guid companyId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var logs = await _auditLogRepository.GetByCompanyAsync(Guid.Empty, cancellationToken);
        var query = logs.Where(a => a.CompanyId == companyId);

        if (from.HasValue) query = query.Where(a => a.Timestamp >= from.Value);
        if (to.HasValue) query = query.Where(a => a.Timestamp <= to.Value);

        return query.OrderByDescending(a => a.Timestamp)
            .Select(a => new AuditLogEntry
            {
                UserId = a.UserId,
                CompanyId = a.CompanyId,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                OldValue = a.OldValue,
                NewValue = a.NewValue,
                IpAddress = a.IpAddress,
                UserAgent = a.UserAgent
            })
            .ToList();
    }
}
