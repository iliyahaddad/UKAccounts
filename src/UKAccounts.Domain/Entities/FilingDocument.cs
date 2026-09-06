namespace UKAccounts.Domain.Entities;

public class FilingDocument : Entity
{
    public Guid FilingId { get; set; }
    public Filing Filing { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Hash { get; set; } = string.Empty;
    public DocumentType Type { get; set; }
}

public enum DocumentType
{
    Ixbrl,
    Xhtml,
    Pdf,
    Other
}
