using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKAccounts.Domain.Entities;

namespace UKAccounts.Infrastructure.Persistence.Configurations;

public class BankStatementConfiguration : IEntityTypeConfiguration<BankStatement>
{
    public void Configure(EntityTypeBuilder<BankStatement> builder)
    {
        builder.HasIndex(s => new { s.BankAccountId, s.StartDate });
        builder.HasIndex(s => s.FileHash);
    }
}
