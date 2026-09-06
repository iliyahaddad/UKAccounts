using Microsoft.Extensions.Logging;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;

namespace UKAccounts.UK;

public class AccountsProductionService
{
    private readonly DormantAccountsGenerator _dormantGenerator;
    private readonly MicroEntityAccountsGenerator _microEntityGenerator;
    private readonly SmallCompanyAccountsGenerator _smallCompanyGenerator;
    private readonly ILogger<AccountsProductionService> _logger;

    public AccountsProductionService(
        DormantAccountsGenerator dormantGenerator,
        MicroEntityAccountsGenerator microEntityGenerator,
        SmallCompanyAccountsGenerator smallCompanyGenerator,
        ILogger<AccountsProductionService> logger)
    {
        _dormantGenerator = dormantGenerator;
        _microEntityGenerator = microEntityGenerator;
        _smallCompanyGenerator = smallCompanyGenerator;
        _logger = logger;
    }

    public async Task<AccountsProductionResult> GenerateAccountsAsync(AccountsProductionRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating accounts for company {CompanyId}, regime {Regime}", request.CompanyId, request.Regime);

        return request.Regime switch
        {
            AccountsRegime.Dormant => await _dormantGenerator.GenerateAsync(request, cancellationToken),
            AccountsRegime.MicroEntity => await _microEntityGenerator.GenerateAsync(request, cancellationToken),
            AccountsRegime.SmallCompany => await _smallCompanyGenerator.GenerateAsync(request, cancellationToken),
            _ => await _smallCompanyGenerator.GenerateAsync(request, cancellationToken)
        };
    }
}
