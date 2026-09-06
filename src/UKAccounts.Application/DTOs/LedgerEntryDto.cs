namespace UKAccounts.Application.DTOs;

public class LedgerEntryDto
{
    public Guid Id { get; set; }
    public Guid JournalId { get; set; }
    public string JournalReference { get; set; } = string.Empty;
    public DateTime JournalDate { get; set; }
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset PostedAt { get; set; }
}
