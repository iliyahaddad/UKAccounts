using System.Globalization;
using Microsoft.Extensions.Logging;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;

namespace UKAccounts.Infrastructure.Import;

public class CsvImportProvider : IImportProvider
{
    private readonly IImportMappingService _mappingService;
    private readonly IBankTransactionService _transactionService;
    private readonly ILogger<CsvImportProvider> _logger;

    public CsvImportProvider(
        IImportMappingService mappingService,
        IBankTransactionService transactionService,
        ILogger<CsvImportProvider> logger)
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
            using var reader = new StreamReader(stream);
            var lines = new List<string[]>();
            string? line;
            var delimiter = DetectDelimiter(reader);

            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                lines.Add(line.Split(delimiter).Select(s => s.Trim('"').Trim()).ToArray());
            }

            if (lines.Count < 2)
            {
                result.Errors.Add("CSV file must contain a header row and at least one data row.");
                result.ErrorCount = 1;
                result.Success = false;
                return result;
            }

            var headers = lines[0];
            var mapping = await _mappingService.GetDefaultAsync(options.CompanyId, "CSV");
            var columnMap = mapping != null ? ParseColumnMappings(mapping.ColumnMappings) : new Dictionary<string, int>();

            for (var i = 1; i < lines.Count; i++)
            {
                try
                {
                    var values = lines[i];
                    if (values.Length != headers.Length) continue;

                    var dateStr = GetValue(values, columnMap, "Date", headers);
                    var description = GetValue(values, columnMap, "Description", headers);
                    var amountStr = GetValue(values, columnMap, "Amount", headers);
                    var debitStr = GetValue(values, columnMap, "Debit", headers);
                    var creditStr = GetValue(values, columnMap, "Credit", headers);
                    var reference = GetValue(values, columnMap, "Reference", headers);
                    var balanceStr = GetValue(values, columnMap, "Balance", headers);

                    if (!DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var transactionDate))
                    {
                        result.Errors.Add($"Row {i}: Invalid date format.");
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
                        SourceDocument = $"CSV Import {DateTime.UtcNow:yyyyMMdd}"
                    };

                    var transaction = await _transactionService.CreateAsync(transactionRequest, cancellationToken);
                    result.Transactions.Add(transaction);
                    result.ImportedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to import row {Row}", i);
                    result.Errors.Add($"Row {i}: {ex.Message}");
                    result.ErrorCount++;
                }
            }

            result.Success = result.ErrorCount == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CSV import failed");
            result.Success = false;
            result.ErrorCount++;
            result.Errors.Add($"Import failed: {ex.Message}");
        }

        return result;
    }

    private static char DetectDelimiter(StreamReader reader)
    {
        var firstLine = reader.ReadLine();
        reader.BaseStream.Seek(0, SeekOrigin.Begin);
        reader.DiscardBufferedData();

        if (firstLine == null) return ',';

        var commaCount = firstLine.Count(c => c == ',');
        var tabCount = firstLine.Count(c => c == '\t');
        var semicolonCount = firstLine.Count(c => c == ';');

        if (tabCount > commaCount && tabCount > semicolonCount) return '\t';
        if (semicolonCount > commaCount) return ';';
        return ',';
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
