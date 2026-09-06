namespace UKAccounts.Application.DTOs;

public class JournalDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid AccountingPeriodId { get; set; }
    public DateTime Date { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public JournalSource Source { get; set; }
    public JournalStatus Status { get; set; }
    public Guid? PostedByUserId { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public Guid? ReversedByJournalId { get; set; }
    public List<JournalLineDto> Lines { get; set; } = new();
    public decimal TotalDebits => Lines.Sum(l => l.Debit);
    public decimal TotalCredits => Lines.Sum(l => l.Credit);
    public bool IsBalanced => TotalDebits == TotalCredits;
}
