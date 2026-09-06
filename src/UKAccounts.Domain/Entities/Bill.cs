namespace UKAccounts.Domain.Entities;

public class Bill : Entity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public Guid SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public string BillNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }
    public string Currency { get; set; } = "GBP";
    public BillStatus Status { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal Total { get; set; }
    public string? Notes { get; set; }
    public Guid? PostedJournalId { get; set; }
    public Journal? PostedJournal { get; set; }
    public ICollection<BillLine> Lines { get; set; } = new List<BillLine>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}

public enum BillStatus
{
    Draft,
    Approved,
    PartiallyPaid,
    Paid,
    Overdue,
    Cancelled
}
