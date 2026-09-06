namespace UKAccounts.Import;

public class ImportProvider : IImportProvider
{
    public Task<ImportResult> ImportAsync(Stream stream, ImportOptions options, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ImportResult { Success = true, ImportedCount = 0 });
    }
}
