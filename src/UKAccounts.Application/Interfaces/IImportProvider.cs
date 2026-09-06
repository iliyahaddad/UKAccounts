using UKAccounts.Application.DTOs;

namespace UKAccounts.Application.Interfaces;

public interface IImportProvider
{
    Task<ImportResult> ImportAsync(Stream stream, ImportOptions options, CancellationToken cancellationToken = default);
}

public class ImportOptions
{
    public Guid CompanyId { get; set; }
    public Guid BankAccountId { get; set; }
    public Guid? ImportMappingId { get; set; }
}

public class ImportResult
{
    public bool Success { get; set; }
    public int ImportedCount { get; set; }
    public int SkippedCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<BankTransactionDto> Transactions { get; set; } = new();
}
