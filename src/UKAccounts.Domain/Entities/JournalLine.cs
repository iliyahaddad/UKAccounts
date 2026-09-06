namespace UKAccounts.Domain.Entities;

public class JournalLine : Entity
{
    public Guid JournalId { get; set; }
    public Journal Journal { get; set; } = null!;
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? TaxCode { get; set; }
    public string? DocumentReference { get; set; }
}
