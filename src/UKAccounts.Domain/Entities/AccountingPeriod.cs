namespace UKAccounts.Domain.Entities;

public class AccountingPeriod : Entity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public DateTime? FiledAt { get; set; }
    public ICollection<Journal> Journals { get; set; } = new List<Journal>();
}
