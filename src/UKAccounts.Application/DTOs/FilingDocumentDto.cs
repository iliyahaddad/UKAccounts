namespace UKAccounts.Application.DTOs;

public class FilingDocumentDto
{
    public Guid Id { get; set; }
    public Guid FilingId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Hash { get; set; } = string.Empty;
    public DocumentType Type { get; set; }
}
