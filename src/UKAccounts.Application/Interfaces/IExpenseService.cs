using UKAccounts.Application.DTOs;

namespace UKAccounts.Application.Interfaces;

public interface IExpenseService
{
    Task<ExpenseDto> CreateAsync(CreateExpenseRequest request, CancellationToken cancellationToken = default);
    Task<ExpenseDto?> GetAsync(Guid expenseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExpenseDto>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<ExpenseDto> UpdateAsync(Guid expenseId, CreateExpenseRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid expenseId, CancellationToken cancellationToken = default);
}
