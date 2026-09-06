using UKAccounts.Application.DTOs;

namespace UKAccounts.Application.Interfaces;

public interface ITrialBalanceService
{
    Task<IReadOnlyList<TrialBalanceDto>> GetTrialBalanceAsync(Guid companyId, DateTime to, CancellationToken cancellationToken = default);
    Task<bool> IsBalancedAsync(Guid companyId, DateTime to, CancellationToken cancellationToken = default);
}
