using Microsoft.Extensions.Logging;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;

namespace UKAccounts.Infrastructure.Services;

public class BankTransactionService : IBankTransactionService
{
    private readonly IRepository<BankTransaction> _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BankTransactionService> _logger;

    public BankTransactionService(
        IRepository<BankTransaction> transactionRepository,
        IUnitOfWork unitOfWork,
        ILogger<BankTransactionService> logger)
    {
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BankTransactionDto> CreateAsync(CreateBankTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var transaction = new BankTransaction
        {
            CompanyId = request.CompanyId,
            BankAccountId = request.BankAccountId,
            TransactionDate = request.TransactionDate,
            ValueDate = request.ValueDate,
            Description = request.Description,
            Reference = request.Reference,
            Amount = request.Amount,
            Type = request.Type,
            Balance = request.Balance,
            SourceDocument = request.SourceDocument
        };

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Bank transaction created {TransactionId}", transaction.Id);
        return ToDto(transaction);
    }

    public async Task<BankTransactionDto?> GetAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId, Guid.Empty, cancellationToken);
        return transaction == null ? null : ToDto(transaction);
    }

    public async Task<IReadOnlyList<BankTransactionDto>> GetByBankAccountAsync(Guid bankAccountId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var transactions = await _transactionRepository.GetByCompanyAsync(Guid.Empty, cancellationToken);
        var filtered = transactions
            .Where(t => t.BankAccountId == bankAccountId && t.TransactionDate >= from && t.TransactionDate <= to)
            .OrderBy(t => t.TransactionDate)
            .ToList();

        return filtered.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<BankTransactionDto>> GetUnreconciledAsync(Guid bankAccountId, CancellationToken cancellationToken = default)
    {
        var transactions = await _transactionRepository.GetByCompanyAsync(Guid.Empty, cancellationToken);
        var filtered = transactions
            .Where(t => t.BankAccountId == bankAccountId && !t.IsReconciled)
            .OrderBy(t => t.TransactionDate)
            .ToList();

        return filtered.Select(ToDto).ToList();
    }

    public async Task<BankTransactionDto> ReconcileAsync(Guid transactionId, Guid matchedJournalId, CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Transaction not found.");

        if (transaction.IsReconciled)
        {
            throw new InvalidOperationException("Transaction is already reconciled.");
        }

        transaction.IsReconciled = true;
        transaction.ReconciledAt = DateTimeOffset.UtcNow;
        transaction.ReconciledByUserId = null;
        transaction.MatchedJournalId = matchedJournalId;

        await _transactionRepository.UpdateAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Transaction reconciled {TransactionId}", transactionId);
        return ToDto(transaction);
    }

    public async Task<BankTransactionDto> UnreconcileAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Transaction not found.");

        if (!transaction.IsReconciled)
        {
            throw new InvalidOperationException("Transaction is not reconciled.");
        }

        transaction.IsReconciled = false;
        transaction.ReconciledAt = null;
        transaction.ReconciledByUserId = null;
        transaction.MatchedJournalId = null;

        await _transactionRepository.UpdateAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Transaction unreconciled {TransactionId}", transactionId);
        return ToDto(transaction);
    }

    private static BankTransactionDto ToDto(BankTransaction transaction)
    {
        return new BankTransactionDto
        {
            Id = transaction.Id,
            CompanyId = transaction.CompanyId,
            BankAccountId = transaction.BankAccountId,
            TransactionDate = transaction.TransactionDate,
            ValueDate = transaction.ValueDate,
            Description = transaction.Description,
            Reference = transaction.Reference,
            Amount = transaction.Amount,
            Type = transaction.Type,
            Balance = transaction.Balance,
            IsReconciled = transaction.IsReconciled,
            SourceDocument = transaction.SourceDocument
        };
    }
}
