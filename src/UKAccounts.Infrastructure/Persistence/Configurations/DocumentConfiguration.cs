using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKAccounts.Domain.Entities;

namespace UKAccounts.Infrastructure.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.HasIndex(d => new { d.CompanyId, d.RelatedEntityType, d.RelatedEntityId });
        builder.HasIndex(d => d.Hash);
    }
}
