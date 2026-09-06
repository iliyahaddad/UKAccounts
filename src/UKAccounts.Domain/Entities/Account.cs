namespace UKAccounts.Domain.Entities;

public class Account : Entity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AccountCategory Category { get; set; }
    public AccountType Type { get; set; }
    public string? ParentCode { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public ICollection<JournalLine> JournalLines { get; set; } = new List<JournalLine>();
}

public enum AccountCategory
{
    Assets,
    Liabilities,
    Equity,
    Income,
    CostOfSales,
    OperatingExpenses,
    Tax
}

public enum AccountType
{
    Asset,
    Liability,
    Equity,
    Income,
    Expense
}
