using UKAccounts.Domain.Entities;

namespace UKAccounts.Application.DTOs;

public class CreateCompanyRequest
{
    public string CompanyNumber { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? RegisteredOffice { get; set; }
    public string? CompanyType { get; set; }
    public DateTime? IncorporationDate { get; set; }
    public string? SicCode { get; set; }
    public DateTime AccountingReferenceDate { get; set; }
    public string Currency { get; set; } = "GBP";
    public AccountsRegime Regime { get; set; } = AccountsRegime.Dormant;
}
