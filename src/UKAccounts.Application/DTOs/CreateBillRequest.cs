namespace UKAccounts.Application.DTOs;

public class CreateBillRequest
{
    public Guid CompanyId { get; set; }
    public Guid SupplierId { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }
    public string Currency { get; set; } = "GBP";
    public string? Notes { get; set; }
    public List<CreateBillLineRequest> Lines { get; set; } = new();
}
