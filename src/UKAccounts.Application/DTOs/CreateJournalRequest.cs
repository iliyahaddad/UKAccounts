namespace UKAccounts.Application.DTOs;

public class CreateJournalRequest
{
    public Guid CompanyId { get; set; }
    public Guid AccountingPeriodId { get; set; }
    public DateTime Date { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public JournalSource Source { get; set; } = JournalSource.Manual;
    public List<CreateJournalLineRequest> Lines { get; set; } = new();
}
