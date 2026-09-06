namespace UKAccounts.Domain.Entities;

public class Company : Entity
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
    public bool IsDormant { get; set; }
    public ICollection<AccountingPeriod> AccountingPeriods { get; set; } = new List<AccountingPeriod>();
    public ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
}

public enum AccountsRegime
{
    Dormant,
    MicroEntity,
    SmallCompany
}
