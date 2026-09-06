namespace UKAccounts.Domain.Entities;

public class Filing : Entity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public Guid AccountingPeriodId { get; set; }
    public AccountingPeriod AccountingPeriod { get; set; } = null!;
    public string FilingType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? GeneratedFileHash { get; set; }
    public DateTimeOffset? SubmissionDate { get; set; }
    public string? SubmissionId { get; set; }
    public string? Response { get; set; }
    public Guid? UserId { get; set; }
    public string? ValidationResult { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<FilingDocument> Documents { get; set; } = new List<FilingDocument>();
    public ICollection<FilingAttempt> Attempts { get; set; } = new List<FilingAttempt>();
}
