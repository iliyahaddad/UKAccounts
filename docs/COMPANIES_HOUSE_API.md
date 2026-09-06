# Companies House API

## 1. Overview

Companies House provides a Software Filing API that allows approved software to submit company accounts and other filings electronically.

**Official Documentation:**
- https://www.gov.uk/software-company-accounts
- https://www.gov.uk/government/collections/companies-house-software-filing
- https://www.gov.uk/government/publications/technical-interface-specifications-for-companies-house-software

## 2. Prerequisites

Before using the Software Filing API, the user must:
1. Know what type of accounts they are filing
2. Have their company authentication code
3. Apply for a presenter account with Companies House

## 3. Authentication

### Presenter Account
- Required for software filing
- Apply via Companies House portal
- Receives a Presenter ID

### Company Authentication Code
- Company-specific code for authentication
- Used to identify the company being filed for
- Not the same as the presenter credentials

### API Authentication
- HTTP Basic Auth or token-based
- Presenter ID + authentication code
- HTTPS mandatory

## 4. API Endpoints (Current)

### Get Company Information
- **Method:** GET
- **Purpose:** Retrieve company details to verify authentication and data
- **Input:** Company number
- **Output:** Company profile JSON

### Submit Accounts
- **Method:** POST
- **Purpose:** Submit iXBRL accounts filing
- **Input:** iXBRL document (XHTML with embedded XBRL)
- **Output:** Submission ID, acceptance/rejection status

### Get Submission Status
- **Method:** GET
- **Purpose:** Check status of a previous submission
- **Input:** Submission ID
- **Output:** Status (PENDING, ACCEPTED, REJECTED, etc.)

## 5. Filing Workflow

```
1. User confirms company and accounting period
2. Application generates iXBRL accounts
3. Application validates iXBRL (Arelle)
4. User reviews and confirms submission
5. Application authenticates with CH API
6. Application submits iXBRL
7. CH returns submission ID
8. Application polls status (if supported)
9. Application stores result and audit trail
```

## 6. Accepted Document Types

- Dormant accounts
- Micro-entity accounts (FRS-105)
- Small company accounts (FRS-102 reduced disclosure)
- Full accounts (FRS-102)
- Abridged accounts (where permitted)

## 7. iXBRL Requirements

- Must conform to accepted UK taxonomies
- Must be valid iXBRL (validated by Arelle)
- Must include all required tags for the accounts type
- Must be a single HTML/XHTML file or document set

## 8. Response Codes and Statuses

| Status | Meaning |
|--------|---------|
| PENDING | Submission received, processing |
| ACCEPTED | Filing accepted by Companies House |
| REJECTED | Filing rejected, see response details |
| FAILED | Technical failure, retry may be appropriate |

## 9. Error Handling

### Common Rejection Reasons
- Invalid iXBRL syntax
- Missing required tags
- Invalid taxonomy reference
- Authentication failure
- Company authentication code mismatch
- Duplicate submission

### Application Behavior
- Never silently retry failed submissions
- Display rejection reasons to user in plain language
- Store full response in audit log
- Allow user to correct and resubmit

## 10. Mock Implementation

### MockCompaniesHouseClient

Implement a mock client for development and testing:

```csharp
public class MockCompaniesHouseClient : ICompaniesHouseClient
{
    public Task<CompanyProfile> GetCompanyAsync(string companyNumber, CancellationToken ct)
    {
        // Return test data or simulate failure
    }

    public Task<SubmissionResult> SubmitAccountsAsync(string companyNumber, Stream ixbrl, CancellationToken ct)
    {
        // Simulate success, rejection, network failure, timeout, auth failure
    }

    public Task<SubmissionStatus> GetSubmissionStatusAsync(string submissionId, CancellationToken ct)
    {
        // Return simulated status
    }
}
```

### Simulation Scenarios
1. **Fake success** — return ACCEPTED with valid submission ID
2. **Fake rejection** — return REJECTED with structured error messages
3. **Fake network failure** — throw `HttpRequestException`
4. **Fake timeout** — return task that never completes or cancels
5. **Fake auth failure** — return 401 Unauthorized

## 11. Rate Limiting and Resilience

- Implement exponential backoff for retries
- Respect `Retry-After` headers if provided
- Timeout per request: 30-60 seconds
- Circuit breaker for repeated failures
- User notification for service outages

## 12. Credential Storage

### Development / Test
- Store in user secrets or environment variables
- Never commit to source control

### Production
- Use Windows Credential Manager or DPAPI
- Encrypt at rest
- Never log credentials
- Never display full credentials in UI

## 13. Security Requirements

- All API calls over HTTPS
- Validate server certificates (do not disable TLS validation)
- No self-signed certificates in production
- No credentials in appsettings.json
- No API keys in logs

## 14. Future Considerations

- **2028 Changes:** Monitor Companies House announcements regarding accounts-filing changes for 2028 and beyond.
- **Multi-filing Support:** Future versions may support confirmation statements, officer changes, etc.
- **Web Migration:** API client abstraction must be reusable in future web version.

## 15. Implementation Checklist

- [ ] Create `ICompaniesHouseClient` interface
- [ ] Implement `MockCompaniesHouseClient`
- [ ] Implement `CompaniesHouseClient` with real API calls
- [ ] Implement credential storage using DPAPI
- [ ] Create company lookup feature
- [ ] Implement accounts submission workflow
- [ ] Implement submission status polling
- [ ] Implement filing history storage
- [ ] Add comprehensive error handling
- [ ] Add audit logging for all filing operations
- [ ] Write integration tests with mock client
- [ ] Test with real credentials only after all mock tests pass
