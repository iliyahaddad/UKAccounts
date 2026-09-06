using UKAccounts.Application.DTOs;

namespace UKAccounts.Application.Interfaces;

public interface IAccountsGenerator
{
    Task<AccountsProductionResult> GenerateAsync(AccountsProductionRequest request, CancellationToken cancellationToken = default);
}
