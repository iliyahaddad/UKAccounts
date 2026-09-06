namespace UKAccounts.Application.DTOs;

public class CashFlowReport
{
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<CashFlowSection> Sections { get; set; } = new();
    public decimal NetCashFromOperating => Sections.FirstOrDefault(s => s.Type == CashFlowType.Operating)?.Total ?? 0;
    public decimal NetCashFromInvesting => Sections.FirstOrDefault(s => s.Type == CashFlowType.Investing)?.Total ?? 0;
    public decimal NetCashFromFinancing => Sections.FirstOrDefault(s => s.Type == CashFlowType.Financing)?.Total ?? 0;
    public decimal NetCashChange => NetCashFromOperating + NetCashFromInvesting + NetCashFromFinancing;
}

public class CashFlowSection
{
    public string Title { get; set; } = string.Empty;
    public CashFlowType Type { get; set; }
    public List<CashFlowLine> Lines { get; set; } = new();
    public decimal Total => Lines.Sum(l => l.Amount);
}

public class CashFlowLine
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public enum CashFlowType
{
    Operating,
    Investing,
    Financing
}
