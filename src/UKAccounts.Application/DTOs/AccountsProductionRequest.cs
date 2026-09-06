namespace UKAccounts.Application.DTOs;

public class AccountsProductionRequest
{
    public Guid CompanyId { get; set; }
    public Guid AccountingPeriodId { get; set; }
    public AccountsRegime Regime { get; set; }
    public bool IncludeDirectorsReport { get; set; }
    public bool IncludeNotes { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

public class AccountsProductionResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? IxbrlPath { get; set; }
    public string? HtmlPath { get; set; }
    public string? PdfPath { get; set; }
    public List<string> Warnings { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
