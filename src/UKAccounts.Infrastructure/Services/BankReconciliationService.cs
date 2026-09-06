using Microsoft.Extensions.Logging;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;

namespace UKAccounts.Infrastructure.Services;

public class BankReconciliationService
{
    private readonly IBankTransactionService _bankTransactionService;
    private readonly IInvoiceService _invoiceService;
    private readonly IBillService _billService;
    private readonly IExpenseService _expenseService;
    private readonly ILogger<BankReconciliationService> _logger;

    public BankReconciliationService(
        IBankTransactionService bankTransactionService,
        IInvoiceService invoiceService,
        IBillService billService,
        IExpenseService expenseService,
        ILogger<BankReconciliationService> logger)
    {
        _bankTransactionService = bankTransactionService;
        _invoiceService = invoiceService;
        _billService = billService;
        _expenseService = expenseService;
        _logger = logger;
    }

    public async Task<ReconciliationResult> ReconcileAsync(Guid transactionId, Guid matchedEntityId, string matchedEntityType, CancellationToken cancellationToken = default)
    {
        var result = new ReconciliationResult { Success = true };

        try
        {
            await _bankTransactionService.ReconcileAsync(transactionId, matchedEntityId, cancellationToken);
            result.Message = "Transaction reconciled successfully.";
            _logger.LogInformation("Transaction {TransactionId} reconciled to {EntityType} {EntityId}", transactionId, matchedEntityType, matchedEntityId);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Reconciliation failed: {ex.Message}";
            _logger.LogWarning(ex, "Failed to reconcile transaction {TransactionId}", transactionId);
        }

        return result;
    }

    public async Task<ReconciliationResult> UnreconcileAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        var result = new ReconciliationResult { Success = true };

        try
        {
            await _bankTransactionService.UnreconcileAsync(transactionId, cancellationToken);
            result.Message = "Transaction unreconciled successfully.";
            _logger.LogInformation("Transaction {TransactionId} unreconciled", transactionId);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Unreconciliation failed: {ex.Message}";
            _logger.LogWarning(ex, "Failed to unreconcile transaction {TransactionId}", transactionId);
        }

        return result;
    }

    public async Task<IReadOnlyList<ReconciliationSuggestion>> GetSuggestionsAsync(Guid bankAccountId, CancellationToken cancellationToken = default)
    {
        var suggestions = new List<ReconciliationSuggestion>();
        var unreconciledTransactions = await _bankTransactionService.GetUnreconciledAsync(bankAccountId, cancellationToken);

        foreach (var transaction in unreconciledTransactions)
        {
            if (transaction.Type == TransactionType.Credit)
            {
                var invoices = await _invoiceService.GetByCompanyAsync(transaction.CompanyId, cancellationToken);
                var matchingInvoice = invoices.FirstOrDefault(i => i.Total == transaction.Amount && i.Status != InvoiceStatus.Paid);
                if (matchingInvoice != null)
                {
                    suggestions.Add(new ReconciliationSuggestion
                    {
                        TransactionId = transaction.Id,
                        TransactionDescription = transaction.Description,
                        TransactionAmount = transaction.Amount,
                        MatchedEntityId = matchingInvoice.Id,
                        MatchedEntityType = "Invoice",
                        MatchedEntityNumber = matchingInvoice.InvoiceNumber,
                        Confidence = 0.9m
                    });
                }
            }
            else
            {
                var bills = await _billService.GetByCompanyAsync(transaction.CompanyId, cancellationToken);
                var matchingBill = bills.FirstOrDefault(b => b.Total == transaction.Amount && b.Status != BillStatus.Paid);
                if (matchingBill != null)
                {
                    suggestions.Add(new ReconciliationSuggestion
                    {
                        TransactionId = transaction.Id,
                        TransactionDescription = transaction.Description,
                        TransactionAmount = transaction.Amount,
                        MatchedEntityId = matchingBill.Id,
                        MatchedEntityType = "Bill",
                        MatchedEntityNumber = matchingBill.BillNumber,
                        Confidence = 0.9m
                    });
                }
            }
        }

        return suggestions;
    }
}

public class ReconciliationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ReconciliationSuggestion
{
    public Guid TransactionId { get; set; }
    public string TransactionDescription { get; set; } = string.Empty;
    public decimal TransactionAmount { get; set; }
    public Guid MatchedEntityId { get; set; }
    public string MatchedEntityType { get; set; } = string.Empty;
    public string MatchedEntityNumber { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
}
