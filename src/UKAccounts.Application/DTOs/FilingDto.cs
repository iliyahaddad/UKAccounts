namespace UKAccounts.Application.DTOs;

public class FilingDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid AccountingPeriodId { get; set; }
    public string FilingType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? GeneratedFileHash { get; set; }
    public DateTimeOffset? SubmissionDate { get; set; }
    public string? SubmissionId { get; set; }
    public string? Response { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<FilingDocumentDto> Documents { get; set; } = new();
    public List<FilingAttemptDto> Attempts { get; set; } = new();
}
