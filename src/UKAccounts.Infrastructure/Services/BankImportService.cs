using Microsoft.Extensions.Logging;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;
using UKAccounts.Infrastructure.Import;

namespace UKAccounts.Infrastructure.Services;

public class BankImportService
{
    private readonly IBankAccountService _bankAccountService;
    private readonly IBankStatementService _bankStatementService;
    private readonly IImportMappingService _mappingService;
    private readonly IBankTransactionService _bankTransactionService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BankImportService> _logger;
    private readonly CsvImportProvider _csvProvider;
    private readonly ExcelImportProvider _excelProvider;

    public BankImportService(
        IBankAccountService bankAccountService,
        IBankStatementService bankStatementService,
        IImportMappingService mappingService,
        IBankTransactionService bankTransactionService,
        IUnitOfWork unitOfWork,
        ILogger<BankImportService> logger)
    {
        _bankAccountService = bankAccountService;
        _bankStatementService = bankStatementService;
        _mappingService = mappingService;
        _bankTransactionService = bankTransactionService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _csvProvider = new CsvImportProvider(mappingService, bankTransactionService, logger);
        _excelProvider = new ExcelImportProvider(mappingService, bankTransactionService, logger);
    }

    public async Task<ImportResult> ImportCsvAsync(Guid companyId, Guid bankAccountId, Stream stream, CancellationToken cancellationToken = default)
    {
        var options = new ImportOptions { CompanyId = companyId, BankAccountId = bankAccountId };
        return await _csvProvider.ImportAsync(stream, options, cancellationToken);
    }

    public async Task<ImportResult> ImportExcelAsync(Guid companyId, Guid bankAccountId, Stream stream, CancellationToken cancellationToken = default)
    {
        var options = new ImportOptions { CompanyId = companyId, BankAccountId = bankAccountId };
        return await _excelProvider.ImportAsync(stream, options, cancellationToken);
    }
}
