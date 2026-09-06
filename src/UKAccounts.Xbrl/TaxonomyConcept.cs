namespace UKAccounts.Xbrl;

public class TaxonomyConcept
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string? PeriodType { get; set; }
    public string? Balance { get; set; }
    public string Taxonomy { get; set; } = string.Empty;
}
