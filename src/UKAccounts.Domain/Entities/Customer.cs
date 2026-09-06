namespace UKAccounts.Domain.Entities;

public class Customer : Entity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? ContactPerson { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
