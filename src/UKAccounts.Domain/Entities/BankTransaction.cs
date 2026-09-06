namespace UKAccounts.Domain.Entities;

public class BankTransaction : Entity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public Guid BankAccountId { get; set; }
    public BankAccount BankAccount { get; set; } = null!;
    public Guid? BankStatementId { get; set; }
    public BankStatement? BankStatement { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime? ValueDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public decimal Balance { get; set; }
    public bool IsReconciled { get; set; }
    public DateTimeOffset? ReconciledAt { get; set; }
    public Guid? ReconciledByUserId { get; set; }
    public string? SourceDocument { get; set; }
    public Guid? MatchedJournalId { get; set; }
    public Journal? MatchedJournal { get; set; }
}

public enum TransactionType
{
    Debit,
    Credit
}
