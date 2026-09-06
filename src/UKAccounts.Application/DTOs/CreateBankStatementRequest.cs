namespace UKAccounts.Application.DTOs;

public class CreateBankStatementRequest
{
    public Guid CompanyId { get; set; }
    public Guid BankAccountId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileHash { get; set; } = string.Empty;
    public Guid? ImportedByUserId { get; set; }
}
