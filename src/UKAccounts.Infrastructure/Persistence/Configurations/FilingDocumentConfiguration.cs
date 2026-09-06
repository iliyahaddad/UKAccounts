using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKAccounts.Domain.Entities;

namespace UKAccounts.Infrastructure.Persistence.Configurations;

public class FilingDocumentConfiguration : IEntityTypeConfiguration<FilingDocument>
{
    public void Configure(EntityTypeBuilder<FilingDocument> builder)
    {
        builder.HasIndex(fd => new { fd.FilingId, fd.Type });
        builder.Property(fd => fd.FileName).IsRequired();
        builder.Property(fd => fd.Hash).IsRequired();
    }
}
