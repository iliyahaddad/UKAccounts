using Microsoft.Extensions.Logging;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;

namespace UKAccounts.Infrastructure.Services;

public class BankStatementService : IBankStatementService
{
    private readonly IRepository<BankStatement> _statementRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BankStatementService> _logger;

    public BankStatementService(
        IRepository<BankStatement> statementRepository,
        IUnitOfWork unitOfWork,
        ILogger<BankStatementService> logger)
    {
        _statementRepository = statementRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BankStatementDto> CreateAsync(CreateBankStatementRequest request, CancellationToken cancellationToken = default)
    {
        var statement = new BankStatement
        {
            CompanyId = request.CompanyId,
            BankAccountId = request.BankAccountId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            FileName = request.FileName,
            FilePath = request.FilePath,
            FileSize = request.FileSize,
            FileHash = request.FileHash,
            Status = StatementStatus.Imported,
            ImportedAt = DateTimeOffset.UtcNow,
            ImportedByUserId = request.ImportedByUserId
        };

        await _statementRepository.AddAsync(statement, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Bank statement created {StatementId}", statement.Id);
        return ToDto(statement);
    }

    public async Task<BankStatementDto?> GetAsync(Guid statementId, CancellationToken cancellationToken = default)
    {
        var statement = await _statementRepository.GetByIdAsync(statementId, Guid.Empty, cancellationToken);
        return statement == null ? null : ToDto(statement);
    }

    public async Task<IReadOnlyList<BankStatementDto>> GetByBankAccountAsync(Guid bankAccountId, CancellationToken cancellationToken = default)
    {
        var statements = await _statementRepository.GetByCompanyAsync(Guid.Empty, cancellationToken);
        var filtered = statements.Where(s => s.BankAccountId == bankAccountId).OrderByDescending(s => s.StartDate).ToList();
        return filtered.Select(ToDto).ToList();
    }

    public async Task<BankStatementDto> UpdateStatusAsync(Guid statementId, StatementStatus status, string? errorMessage = null, CancellationToken cancellationToken = default)
    {
        var statement = await _statementRepository.GetByIdAsync(statementId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Statement not found.");

        statement.Status = status;
        statement.ErrorMessage = errorMessage;

        await _statementRepository.UpdateAsync(statement, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Bank statement status updated {StatementId} to {Status}", statementId, status);
        return ToDto(statement);
    }

    private static BankStatementDto ToDto(BankStatement statement)
    {
        return new BankStatementDto
        {
            Id = statement.Id,
            CompanyId = statement.CompanyId,
            BankAccountId = statement.BankAccountId,
            StartDate = statement.StartDate,
            EndDate = statement.EndDate,
            FileName = statement.FileName,
            FileSize = statement.FileSize,
            FileHash = statement.FileHash,
            Status = statement.Status,
            ImportedTransactionCount = statement.ImportedTransactionCount,
            MatchedTransactionCount = statement.MatchedTransactionCount,
            ImportedAt = statement.ImportedAt,
            ErrorMessage = statement.ErrorMessage
        };
    }
}
