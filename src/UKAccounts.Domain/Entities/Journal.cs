namespace UKAccounts.Domain.Entities;

public class Journal : Entity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public Guid AccountingPeriodId { get; set; }
    public AccountingPeriod AccountingPeriod { get; set; } = null!;
    public DateTime Date { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public JournalSource Source { get; set; }
    public JournalStatus Status { get; set; }
    public Guid? PostedByUserId { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public Guid? ReversedByJournalId { get; set; }
    public ICollection<JournalLine> Lines { get; set; } = new List<JournalLine>();
}

public enum JournalSource
{
    Manual,
    Invoice,
    Bill,
    Expense,
    BankImport,
    System
}

public enum JournalStatus
{
    Draft,
    Posted,
    Reversed
}
