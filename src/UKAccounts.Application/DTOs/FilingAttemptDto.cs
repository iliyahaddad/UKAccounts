namespace UKAccounts.Application.DTOs;

public class FilingAttemptDto
{
    public Guid Id { get; set; }
    public Guid FilingId { get; set; }
    public DateTimeOffset AttemptedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Response { get; set; }
}
