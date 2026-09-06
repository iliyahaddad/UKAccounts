namespace UKAccounts.Application.DTOs;

public class CreateInvoiceRequest
{
    public Guid CompanyId { get; set; }
    public Guid CustomerId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }
    public string Currency { get; set; } = "GBP";
    public string? Notes { get; set; }
    public List<CreateInvoiceLineRequest> Lines { get; set; } = new();
}
