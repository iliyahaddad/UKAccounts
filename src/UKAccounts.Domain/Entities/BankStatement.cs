namespace UKAccounts.Domain.Entities;

public class BankStatement : Entity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public Guid BankAccountId { get; set; }
    public BankAccount BankAccount { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileHash { get; set; } = string.Empty;
    public StatementStatus Status { get; set; }
    public int ImportedTransactionCount { get; set; }
    public int MatchedTransactionCount { get; set; }
    public DateTimeOffset ImportedAt { get; set; }
    public Guid? ImportedByUserId { get; set; }
    public string? ErrorMessage { get; set; }
    public ICollection<BankTransaction> Transactions { get; set; } = new List<BankTransaction>();
}

public enum StatementStatus
{
    Imported,
    Processing,
    Completed,
    Failed,
    PartiallyMatched
}
