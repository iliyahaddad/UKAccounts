namespace UKAccounts.Application.DTOs;

public class CreateBankTransactionRequest
{
    public Guid CompanyId { get; set; }
    public Guid BankAccountId { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime? ValueDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public decimal Balance { get; set; }
    public string? SourceDocument { get; set; }
}
