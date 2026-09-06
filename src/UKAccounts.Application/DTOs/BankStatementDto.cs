namespace UKAccounts.Application.DTOs;

public class BankStatementDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid BankAccountId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileHash { get; set; } = string.Empty;
    public StatementStatus Status { get; set; }
    public int ImportedTransactionCount { get; set; }
    public int MatchedTransactionCount { get; set; }
    public DateTimeOffset ImportedAt { get; set; }
    public string? ErrorMessage { get; set; }
}
