namespace UKAccounts.Application.DTOs;

public class AccountingPeriodDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public DateTime? FiledAt { get; set; }
}
