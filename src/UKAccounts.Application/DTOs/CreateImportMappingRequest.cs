namespace UKAccounts.Application.DTOs;

public class CreateImportMappingRequest
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SourceFormat { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string ColumnMappings { get; set; } = string.Empty;
}
