namespace UKAccounts.Application.DTOs;

public class CreateJournalLineRequest
{
    public Guid AccountId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? TaxCode { get; set; }
    public string? DocumentReference { get; set; }
}
