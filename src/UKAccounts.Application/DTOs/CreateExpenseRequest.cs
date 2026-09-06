namespace UKAccounts.Application.DTOs;

public class CreateExpenseRequest
{
    public Guid CompanyId { get; set; }
    public Guid? SupplierId { get; set; }
    public DateTime Date { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? TaxCode { get; set; }
    public decimal TaxAmount { get; set; }
    public string PaymentAccount { get; set; } = string.Empty;
    public string? ReceiptDocumentId { get; set; }
    public string? Notes { get; set; }
}
