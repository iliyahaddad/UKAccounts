using Microsoft.Extensions.Logging;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;

namespace UKAccounts.Infrastructure.Services;

public class ExpenseService : IExpenseService
{
    private readonly IRepository<Expense> _expenseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExpenseService> _logger;

    public ExpenseService(
        IRepository<Expense> expenseRepository,
        IUnitOfWork unitOfWork,
        ILogger<ExpenseService> logger)
    {
        _expenseRepository = expenseRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ExpenseDto> CreateAsync(CreateExpenseRequest request, CancellationToken cancellationToken = default)
    {
        var expense = new Expense
        {
            CompanyId = request.CompanyId,
            SupplierId = request.SupplierId,
            Date = request.Date,
            Category = request.Category,
            Description = request.Description,
            Amount = request.Amount,
            TaxCode = request.TaxCode,
            TaxAmount = request.TaxAmount,
            PaymentAccount = request.PaymentAccount,
            ReceiptDocumentId = request.ReceiptDocumentId,
            Notes = request.Notes
        };

        await _expenseRepository.AddAsync(expense, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Expense created {ExpenseId}", expense.Id);
        return ToDto(expense);
    }

    public async Task<ExpenseDto?> GetAsync(Guid expenseId, CancellationToken cancellationToken = default)
    {
        var expense = await _expenseRepository.GetByIdAsync(expenseId, Guid.Empty, cancellationToken);
        return expense == null ? null : ToDto(expense);
    }

    public async Task<IReadOnlyList<ExpenseDto>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var expenses = await _expenseRepository.GetByCompanyAsync(companyId, cancellationToken);
        return expenses.Select(ToDto).ToList();
    }

    public async Task<ExpenseDto> UpdateAsync(Guid expenseId, CreateExpenseRequest request, CancellationToken cancellationToken = default)
    {
        var expense = await _expenseRepository.GetByIdAsync(expenseId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Expense not found.");

        expense.SupplierId = request.SupplierId;
        expense.Date = request.Date;
        expense.Category = request.Category;
        expense.Description = request.Description;
        expense.Amount = request.Amount;
        expense.TaxCode = request.TaxCode;
        expense.TaxAmount = request.TaxAmount;
        expense.PaymentAccount = request.PaymentAccount;
        expense.ReceiptDocumentId = request.ReceiptDocumentId;
        expense.Notes = request.Notes;

        await _expenseRepository.UpdateAsync(expense, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Expense updated {ExpenseId}", expense.Id);
        return ToDto(expense);
    }

    public async Task DeleteAsync(Guid expenseId, CancellationToken cancellationToken = default)
    {
        var expense = await _expenseRepository.GetByIdAsync(expenseId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Expense not found.");

        await _expenseRepository.DeleteAsync(expense, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Expense deleted {ExpenseId}", expenseId);
    }

    private static ExpenseDto ToDto(Expense expense)
    {
        return new ExpenseDto
        {
            Id = expense.Id,
            CompanyId = expense.CompanyId,
            SupplierId = expense.SupplierId,
            SupplierName = expense.Supplier?.Name,
            Date = expense.Date,
            Category = expense.Category,
            Description = expense.Description,
            Amount = expense.Amount,
            TaxCode = expense.TaxCode,
            TaxAmount = expense.TaxAmount,
            Total = expense.Amount + expense.TaxAmount,
            PaymentAccount = expense.PaymentAccount,
            ReceiptDocumentId = expense.ReceiptDocumentId,
            Notes = expense.Notes
        };
    }
}
