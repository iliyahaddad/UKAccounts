using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKAccounts.Domain.Entities;

namespace UKAccounts.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasIndex(a => new { a.UserId, a.Timestamp });
        builder.HasIndex(a => new { a.CompanyId, a.Timestamp });
        builder.Property(a => a.Action).IsRequired().HasMaxLength(256);
        builder.Property(a => a.EntityType).IsRequired().HasMaxLength(256);
    }
}
