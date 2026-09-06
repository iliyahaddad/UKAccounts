using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKAccounts.Domain.Entities;

namespace UKAccounts.Infrastructure.Persistence.Configurations;

public class FilingConfiguration : IEntityTypeConfiguration<Filing>
{
    public void Configure(EntityTypeBuilder<Filing> builder)
    {
        builder.HasIndex(f => new { f.CompanyId, f.Status });
        builder.HasIndex(f => f.SubmissionId);
        builder.Property(f => f.FilingType).IsRequired();
        builder.Property(f => f.Status).IsRequired();
    }
}
