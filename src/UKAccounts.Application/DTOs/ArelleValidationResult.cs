namespace UKAccounts.Application.DTOs;

public class ArelleValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationMessage> Errors { get; set; } = new();
    public List<ValidationMessage> Warnings { get; set; } = new();
    public List<ValidationMessage> Infos { get; set; } = new();
    public TimeSpan Duration { get; set; }
    public string ArelleVersion { get; set; } = string.Empty;
}

public class ValidationMessage
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string? FactReference { get; set; }
    public int? LineNumber { get; set; }
}
