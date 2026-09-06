namespace UKAccounts.Domain.Entities;

public class Expense : Entity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public Guid? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public DateTime Date { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? TaxCode { get; set; }
    public decimal TaxAmount { get; set; }
    public string PaymentAccount { get; set; } = string.Empty;
    public string? ReceiptDocumentId { get; set; }
    public Document? ReceiptDocument { get; set; }
    public string? Notes { get; set; }
    public Guid? PostedJournalId { get; set; }
    public Journal? PostedJournal { get; set; }
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
