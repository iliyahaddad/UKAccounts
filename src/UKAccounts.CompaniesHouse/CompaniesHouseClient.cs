namespace UKAccounts.CompaniesHouse;

public interface ICompaniesHouseClient
{
    Task<CompanyProfile?> GetCompanyAsync(string companyNumber, CancellationToken cancellationToken = default);
    Task<SubmissionResult> SubmitAccountsAsync(string companyNumber, Stream ixbrl, CancellationToken cancellationToken = default);
    Task<SubmissionStatus> GetSubmissionStatusAsync(string submissionId, CancellationToken cancellationToken = default);
}
