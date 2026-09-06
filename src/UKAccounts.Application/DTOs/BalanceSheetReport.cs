namespace UKAccounts.Application.DTOs;

public class BalanceSheetReport
{
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public DateTime AsOfDate { get; set; }
    public List<BalanceSheetSection> Sections { get; set; } = new();
    public decimal TotalAssets => Sections.FirstOrDefault(s => s.Type == BalanceSheetSectionType.Assets)?.Total ?? 0;
    public decimal TotalLiabilities => Sections.FirstOrDefault(s => s.Type == BalanceSheetSectionType.Liabilities)?.Total ?? 0;
    public decimal TotalEquity => Sections.FirstOrDefault(s => s.Type == BalanceSheetSectionType.Equity)?.Total ?? 0;
    public bool IsBalanced => Math.Abs(TotalAssets - (TotalLiabilities + TotalEquity)) < 0.01m;
}

public class BalanceSheetSection
{
    public string Title { get; set; } = string.Empty;
    public BalanceSheetSectionType Type { get; set; }
    public List<BalanceSheetLine> Lines { get; set; } = new();
    public decimal Total => Lines.Sum(l => l.Amount);
}

public class BalanceSheetLine
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public enum BalanceSheetSectionType
{
    Assets,
    Liabilities,
    Equity
}
