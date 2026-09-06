namespace UKAccounts.CompaniesHouse;

public class MockCompaniesHouseClient : ICompaniesHouseClient
{
    public Task<CompanyProfile?> GetCompanyAsync(string companyNumber, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<CompanyProfile?>(null);
    }

    public Task<SubmissionResult> SubmitAccountsAsync(string companyNumber, Stream ixbrl, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SubmissionResult { SubmissionId = "MOCK-001", Accepted = true });
    }

    public Task<SubmissionStatus> GetSubmissionStatusAsync(string submissionId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SubmissionStatus { Status = "ACCEPTED" });
    }
}
