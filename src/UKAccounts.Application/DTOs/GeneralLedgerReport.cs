namespace UKAccounts.Application.DTOs;

public class GeneralLedgerReport
{
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public List<GeneralLedgerEntry> Entries { get; set; } = new();
    public decimal TotalDebits => Entries.Sum(e => e.Debit);
    public decimal TotalCredits => Entries.Sum(e => e.Credit);
    public decimal ClosingBalance => OpeningBalance + TotalDebits - TotalCredits;
}

public class GeneralLedgerEntry
{
    public DateTime Date { get; set; }
    public string JournalReference { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}
