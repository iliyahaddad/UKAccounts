namespace UKAccounts.Application.Interfaces;

public interface ICompaniesHouseClient
{
    Task<CompanyProfile?> GetCompanyAsync(string companyNumber, CancellationToken cancellationToken = default);
    Task<SubmissionResult> SubmitAccountsAsync(string companyNumber, Stream ixbrl, CancellationToken cancellationToken = default);
    Task<SubmissionStatus> GetSubmissionStatusAsync(string submissionId, CancellationToken cancellationToken = default);
}

public class CompanyProfile { public string CompanyNumber { get; set; } = string.Empty; public string CompanyName { get; set; } = string.Empty; }
public class SubmissionResult { public string SubmissionId { get; set; } = string.Empty; public bool Accepted { get; set; } }
public class SubmissionStatus { public string Status { get; set; } = string.Empty; }
