namespace UKAccounts.Application.DTOs;

public class BackupResult
{
    public bool Success { get; set; }
    public string? BackupPath { get; set; }
    public string? ErrorMessage { get; set; }
    public long Size { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class RestoreResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? RestoredFromPath { get; set; }
    public DateTimeOffset RestoredAt { get; set; } = DateTimeOffset.UtcNow;
}
