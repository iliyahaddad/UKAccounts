using Microsoft.Extensions.Logging;
using UKAccounts.Application.Interfaces;

namespace UKAccounts.Infrastructure.Services;

public class CompaniesHouseLookupService : ICompaniesHouseLookupService
{
    private readonly ILogger<CompaniesHouseLookupService> _logger;

    public CompaniesHouseLookupService(ILogger<CompaniesHouseLookupService> logger)
    {
        _logger = logger;
    }

    public Task<CompanyLookupResult?> LookupAsync(string companyNumber, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Looking up company {CompanyNumber}", companyNumber);

        if (string.IsNullOrWhiteSpace(companyNumber) || companyNumber.Length != 8)
        {
            return Task.FromResult<CompanyLookupResult?>(null);
        }

        var result = new CompanyLookupResult
        {
            CompanyNumber = companyNumber,
            CompanyName = $"{companyNumber} LIMITED",
            CompanyStatus = "Active",
            CompanyType = "Ltd",
            IncorporationDate = new DateTime(2020, 1, 1),
            RegisteredOffice = "1 Test Street, London, SW1A 1AA",
            SicCode = "62020",
            IsDormant = false,
            Success = true
        };

        return Task.FromResult<CompanyLookupResult?>(result);
    }
}
