using UKAccounts.Domain.Entities;

namespace UKAccounts.Application.DTOs;

public class CompanyDto
{
    public Guid Id { get; set; }
    public string CompanyNumber { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? RegisteredOffice { get; set; }
    public string? CompanyType { get; set; }
    public DateTime? IncorporationDate { get; set; }
    public string? SicCode { get; set; }
    public DateTime AccountingReferenceDate { get; set; }
    public string Currency { get; set; } = "GBP";
    public AccountsRegime Regime { get; set; }
    public bool IsDormant { get; set; }
    public int AccountingPeriodCount { get; set; }
}
