using Microsoft.Extensions.Logging;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;

namespace UKAccounts.Infrastructure.Import;

public class ExcelImportProvider : IImportProvider
{
    private readonly IImportMappingService _mappingService;
    private readonly IBankTransactionService _transactionService;
    private readonly ILogger<ExcelImportProvider> _logger;

    public ExcelImportProvider(
        IImportMappingService mappingService,
        IBankTransactionService transactionService,
        ILogger<ExcelImportProvider> logger)
    {
        _mappingService = mappingService;
        _transactionService = transactionService;
        _logger = logger;
    }

    public async Task<ImportResult> ImportAsync(Stream stream, ImportOptions options, CancellationToken cancellationToken = default)
    {
        var result = new ImportResult { Success = true };

        try
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();
            var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

            var headerRow = worksheet.RangeUsed().FirstRow();
            var headers = headerRow.Cells().Select(c => c.GetString().Trim()).ToArray();

            var mapping = await _mappingService.GetDefaultAsync(options.CompanyId, "Excel");
            var columnMap = mapping != null ? ParseColumnMappings(mapping.ColumnMappings) : new Dictionary<string, int>();

            foreach (var row in rows)
            {
                try
                {
                    var values = row.Cells().Select(c => c.GetString().Trim()).ToArray();

                    var dateStr = GetValue(values, columnMap, "Date", headers);
                    var description = GetValue(values, columnMap, "Description", headers);
                    var amountStr = GetValue(values, columnMap, "Amount", headers);
                    var debitStr = GetValue(values, columnMap, "Debit", headers);
                    var creditStr = GetValue(values, columnMap, "Credit", headers);
                    var reference = GetValue(values, columnMap, "Reference", headers);
                    var balanceStr = GetValue(values, columnMap, "Balance", headers);

                    if (!DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var transactionDate))
                    {
                        result.Errors.Add($"Row {row.RowNumber()}: Invalid date format.");
                        result.ErrorCount++;
                        continue;
                    }

                    decimal amount = 0;
                    decimal balance = 0;

                    if (!string.IsNullOrEmpty(debitStr) && decimal.TryParse(debitStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var debit))
                    {
                        amount = debit;
                    }
                    else if (!string.IsNullOrEmpty(creditStr) && decimal.TryParse(creditStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var credit))
                    {
                        amount = credit;
                    }
                    else if (!string.IsNullOrEmpty(amountStr) && decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedAmount))
                    {
                        amount = parsedAmount;
                    }

                    decimal.TryParse(balanceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out balance);

                    var type = amount >= 0 ? TransactionType.Credit : TransactionType.Debit;
                    if (debitStr.Length > 0) type = TransactionType.Debit;
                    if (creditStr.Length > 0) type = TransactionType.Credit;

                    var transactionRequest = new CreateBankTransactionRequest
                    {
                        CompanyId = options.CompanyId,
                        BankAccountId = options.BankAccountId,
                        TransactionDate = transactionDate,
                        Description = description,
                        Reference = reference,
                        Amount = Math.Abs(amount),
                        Type = type,
                        Balance = balance,
                        SourceDocument = $"Excel Import {DateTime.UtcNow:yyyyMMdd}"
                    };

                    var transaction = await _transactionService.CreateAsync(transactionRequest, cancellationToken);
                    result.Transactions.Add(transaction);
                    result.ImportedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to import row {Row}", row.RowNumber());
                    result.Errors.Add($"Row {row.RowNumber()}: {ex.Message}");
                    result.ErrorCount++;
                }
            }

            result.Success = result.ErrorCount == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excel import failed");
            result.Success = false;
            result.ErrorCount++;
            result.Errors.Add($"Import failed: {ex.Message}");
        }

        return result;
    }

    private static string GetValue(string[] values, Dictionary<string, int> columnMap, string columnName, string[] headers)
    {
        if (columnMap.TryGetValue(columnName, out var index) && index < values.Length)
        {
            return values[index];
        }

        var headerIndex = Array.IndexOf(headers, columnName);
        if (headerIndex >= 0 && headerIndex < values.Length)
        {
            return values[headerIndex];
        }

        return string.Empty;
    }

    private static Dictionary<string, int> ParseColumnMappings(string json)
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(json) ?? new Dictionary<string, int>();
        }
        catch
        {
            return new Dictionary<string, int>();
        }
    }
}
