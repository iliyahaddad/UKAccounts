namespace UKAccounts.Application.Interfaces;

public interface IIxbrlGenerator
{
    Task<GenerationResult> GenerateAsync(IxbrlRequest request, CancellationToken cancellationToken = default);
}

public class IxbrlRequest
{
    public Guid CompanyId { get; set; }
    public Guid AccountingPeriodId { get; set; }
    public string? CompanyNumber { get; set; }
    public string? CompanyName { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public AccountsRegime Regime { get; set; }
}

public class GenerationResult
{
    public bool Success { get; set; }
    public string? OutputPath { get; set; }
    public string? IxbrlContent { get; set; }
    public List<string> Warnings { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
