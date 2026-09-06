namespace UKAccounts.Domain.Entities;

public class FilingAttempt : Entity
{
    public Guid FilingId { get; set; }
    public Filing Filing { get; set; } = null!;
    public DateTimeOffset AttemptedAt { get; set; } = DateTimeOffset.UtcNow;
    public string Status { get; set; } = string.Empty;
    public string? Response { get; set; }
    public Guid? UserId { get; set; }
}
