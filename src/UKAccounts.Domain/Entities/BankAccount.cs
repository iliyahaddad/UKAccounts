namespace UKAccounts.Domain.Entities;

public class BankAccount : Entity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string SortCode { get; set; } = string.Empty;
    public string Currency { get; set; } = "GBP";
    public decimal OpeningBalance { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<BankTransaction> Transactions { get; set; } = new List<BankTransaction>();
    public ICollection<BankStatement> Statements { get; set; } = new List<BankStatement>();
}
