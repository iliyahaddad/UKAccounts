namespace UKAccounts.Application.DTOs;

public class CreateBankAccountRequest
{
    public Guid CompanyId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string SortCode { get; set; } = string.Empty;
    public string Currency { get; set; } = "GBP";
    public decimal OpeningBalance { get; set; }
}
