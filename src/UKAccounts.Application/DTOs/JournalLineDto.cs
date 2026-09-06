namespace UKAccounts.Application.DTOs;

public class JournalLineDto
{
    public Guid Id { get; set; }
    public Guid JournalId { get; set; }
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? TaxCode { get; set; }
    public string? DocumentReference { get; set; }
}
