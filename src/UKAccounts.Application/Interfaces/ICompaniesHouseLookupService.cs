namespace UKAccounts.Application.Interfaces;

public interface ICompaniesHouseLookupService
{
    Task<CompanyLookupResult?> LookupAsync(string companyNumber, CancellationToken cancellationToken = default);
}

public class CompanyLookupResult
{
    public string CompanyNumber { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyStatus { get; set; }
    public string? CompanyType { get; set; }
    public DateTime? IncorporationDate { get; set; }
    public string? RegisteredOffice { get; set; }
    public string? SicCode { get; set; }
    public bool IsDormant { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
