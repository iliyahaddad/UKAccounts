using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKAccounts.Domain.Entities;

namespace UKAccounts.Infrastructure.Persistence.Configurations;

public class FilingAttemptConfiguration : IEntityTypeConfiguration<FilingAttempt>
{
    public void Configure(EntityTypeBuilder<FilingAttempt> builder)
    {
        builder.HasIndex(fa => new { fa.FilingId, fa.AttemptedAt });
        builder.Property(fa => fa.Status).IsRequired();
    }
}
