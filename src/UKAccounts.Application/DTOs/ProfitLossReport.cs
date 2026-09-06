namespace UKAccounts.Application.DTOs;

public class ProfitLossReport
{
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<ProfitLossSection> Sections { get; set; } = new();
    public decimal TotalRevenue => Sections.Where(s => s.Type == ReportSectionType.Revenue).Sum(s => s.Total);
    public decimal TotalExpenses => Sections.Where(s => s.Type == ReportSectionType.Expense).Sum(s => s.Total);
    public decimal NetProfit => TotalRevenue - TotalExpenses;
}

public class ProfitLossSection
{
    public string Title { get; set; } = string.Empty;
    public ReportSectionType Type { get; set; }
    public List<ProfitLossLine> Lines { get; set; } = new();
    public decimal Total => Lines.Sum(l => l.Amount);
}

public class ProfitLossLine
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public enum ReportSectionType
{
    Revenue,
    CostOfSales,
    Expense,
    OtherIncome,
    OtherExpense,
    Tax
}
