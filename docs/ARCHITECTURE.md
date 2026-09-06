# Architecture

## 1. Overview

UKAccounts is a Windows desktop application built on clean architecture principles. The solution is structured to maximize testability, enable future SaaS migration, and enforce strict separation between domain logic, application services, infrastructure, and UI.

```
┌─────────────────────────────────────────────┐
│              Desktop (WPF/MVVM)             │
│  ┌───────────────────────────────────────┐  │
│  │         ViewModels                     │  │
│  └───────────────────────────────────────┘  │
└──────────────────────┬──────────────────────┘
                       │ depends on
                       ▼
┌─────────────────────────────────────────────┐
│           Application Layer                 │
│  ┌───────────────────────────────────────┐  │
│  │   Services, DTOs, Use Cases            │  │
│  └───────────────────────────────────────┘  │
└──────────────────────┬──────────────────────┘
                       │ depends on
                       ▼
┌─────────────────────────────────────────────┐
│              Domain Layer                   │
│  ┌───────────────────────────────────────┐  │
│  │   Entities, Value Objects, Interfaces  │  │
│  │   Business Rules, Domain Events        │  │
│  └───────────────────────────────────────┘  │
└─────────────────────────────────────────────┘
                       ▲
                       │ implemented by
                       │
┌─────────────────────────────────────────────┐
│           Infrastructure Layer              │
│  ┌───────────────────────────────────────┐  │
│  │   EF Core / SQLite Repositories       │  │
│  │   File System, Import Providers       │  │
│  │   External API Clients                │  │
│  └───────────────────────────────────────┘  │
└─────────────────────────────────────────────┘
```

### Future Web Migration Path

```
Web UI (ASP.NET Core / Blazor)
    │
    ▼
ASP.NET Core API
    │
    ▼
Application Layer (unchanged)
    │
    ▼
Domain Layer (unchanged)
    │
    ▼
Infrastructure (PostgreSQL adapter)
```

The accounting engine, reporting engine, UK accounts engine, iXBRL engine, and Companies House service abstraction require minimal changes.

---

## 2. Project Structure

```
UKAccounts.sln
│
├── UKAccounts.Domain/
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Interfaces/
│   ├── Events/
│   └── Exceptions/
│
├── UKAccounts.Application/
│   ├── Interfaces/
│   ├── Services/
│   ├── DTOs/
│   ├── UseCases/
│   └── Validators/
│
├── UKAccounts.Infrastructure/
│   ├── Persistence/
│   │   ├── DbContext
│   │   ├── Repositories
│   │   └── Migrations
│   ├── Import/
│   ├── Security/
│   ├── Storage/
│   └── External/
│
├── UKAccounts.Accounting/
│   ├── ChartOfAccounts/
│   ├── Journals/
│   ├── Ledger/
│   ├── TrialBalance/
│   └── DoubleEntryValidator.cs
│
├── UKAccounts.UK/
│   ├── CompaniesHouse/
│   ├── TaxCodes/
│   ├── AccountsRegimes/
│   └── UKTaxonomy/
│
├── UKAccounts.Xbrl/
│   ├── IIxbrlGenerator.cs
│   ├── IxbrlDocument.cs
│   ├── TaxonomyMapper.cs
│   └── ArelleAdapter/
│
├── UKAccounts.CompaniesHouse/
│   ├── ICompaniesHouseClient.cs
│   ├── CompaniesHouseClient.cs
│   ├── MockCompaniesHouseClient.cs
│   └── Models/
│
├── UKAccounts.Import/
│   ├── IImportProvider.cs
│   ├── CsvImportProvider.cs
│   ├── ExcelImportProvider.cs
│   ├── PdfImportProvider.cs
│   └── Mapping/
│
├── UKAccounts.Security/
│   ├── Authentication/
│   ├── Authorization/
│   ├── PasswordHasher.cs
│   └── AuditLog/
│
├── UKAccounts.Reporting/
│   ├── IReportService.cs
│   ├── ProfitLossReport.cs
│   ├── BalanceSheetReport.cs
│   ├── TrialBalanceReport.cs
│   └── CashFlowReport.cs
│
├── UKAccounts.Desktop/
│   ├── Views/
│   ├── ViewModels/
│   ├── Services/
│   ├── Resources/
│   └── App.xaml
│
├── UKAccounts.ArelleWorker/
│   ├── Program.cs
│   ├── ValidationWorker.cs
│   └── Communication/
│
├── UKAccounts.Tests/
│   ├── Unit/
│   └── Domain/
│
└── UKAccounts.IntegrationTests/
    ├── Accounting/
    ├── Import/
    └── Filing/
```

---

## 3. Layer Responsibilities

### Domain Layer
- **No dependencies** on any other project
- Contains all business entities, value objects, domain events, and business rules
- Defines repository interfaces (`IRepository<T>`, `IUnitOfWork`)
- Defines service interfaces (`IAccountingEngine`, `IReportService`, `ICompaniesHouseClient`)
- No EF Core, no SQLite, no WPF, no HTTP

### Application Layer
- Depends only on Domain
- Contains use case implementations
- Contains DTOs for data transfer
- Contains validation logic (FluentValidation or similar)
- Orchestrates domain objects
- Does not contain UI logic

### Infrastructure Layer
- Depends on Application and Domain
- Implements repository interfaces using EF Core
- Implements external service clients
- Handles file system, import providers, PDF processing
- Contains EF Core DbContext and migrations
- Never leaks infrastructure concerns into Domain or Application

### Accounting Layer
- Depends on Domain
- Contains the double-entry accounting engine
- Chart of accounts management
- Journal posting, reversal
- Ledger, trial balance
- Pure accounting logic, no persistence details

### UK Layer
- Depends on Domain and Accounting
- UK-specific business rules
- Companies House domain models
- Tax codes and rates
- Accounts regime logic

### Xbrl Layer
- Depends on Domain and UK
- iXBRL generation abstractions
- Taxonomy mapping
- Arelle adapter
- Does not depend on Arelle directly (uses adapter)

### CompaniesHouse Layer
- Depends on Domain and UK
- `ICompaniesHouseClient` interface
- Production and mock implementations
- Models for CH API requests/responses

### Import Layer
- Depends on Domain
- Import provider interface and implementations
- CSV, Excel, PDF parsing
- Mapping configuration
- Duplicate detection

### Security Layer
- Depends on Domain
- Password hashing
- Authentication
- Authorization (roles, permissions)
- Audit logging

### Reporting Layer
- Depends on Domain and Accounting
- Financial report generation
- Report DTOs
- Export to PDF/CSV

### Desktop Layer
- Depends on Application and Infrastructure
- WPF Views and ViewModels
- UI-specific services
- Dependency injection configuration
- No business logic in code-behind

### ArelleWorker
- Standalone executable
- Depends on Arelle (Python)
- Receives iXBRL path via command line
- Outputs structured JSON result
- Isolated from main application process

---

## 4. Dependency Injection

Use `Microsoft.Extensions.DependencyInjection` throughout.

### Registration Pattern

```csharp
// In Desktop bootstrapper
services.AddDomain();
services.AddApplication();
services.AddInfrastructure(configuration);
services.AddAccounting();
services.AddUk();
services.AddXbrl();
services.AddCompaniesHouse();
services.AddImport();
services.AddSecurity();
services.AddReporting();
```

### Lifetime Guidelines
- **Scoped** — DbContext, unit of work, per-request services
- **Singleton** — stateless services, configuration, logging
- **Transient** — validators, lightweight factories

---

## 5. Cross-Cutting Concerns

### Logging
- `Microsoft.Extensions.Logging` everywhere
- Separate log categories:
  - `Security` — authentication, authorization
  - `Accounting` — journal entries, postings
  - `Filing` — CH submissions
  - `Import` — file imports
  - `Application` — general
- Never log secrets, passwords, API keys, auth codes

### Error Handling
- No unhandled exceptions
- Result pattern or custom exceptions
- Friendly error messages in UI
- Diagnostics screen for technical details
- `ILogger` for structured error context

### Validation
- FluentValidation in Application layer
- Domain invariants enforced in Domain
- Double-entry invariant: `SUM(Debits) == SUM(Credits)` for posted journals

### Transactions
- EF Core transactions for atomic operations
- Explicit `BeginTransaction()` for multi-step operations
- Rollback on failure

---

## 6. Company Isolation

Every query/service operation must filter by `CompanyId`.

### Repository Pattern with Company Scoping

```csharp
public interface IRepository<T> where T : Entity
{
    Task<T?> GetByIdAsync(Guid id, Guid companyId, CancellationToken ct);
    Task<IReadOnlyList<T>> GetByCompanyAsync(Guid companyId, CancellationToken ct);
    Task AddAsync(T entity, CancellationToken ct);
    // ...
}
```

### Enforcement
- Repository methods require `companyId`
- Service layer passes current company context
- Tests verify cross-company access is impossible
- Never rely on UI filtering alone

---

## 7. Data Isolation Verification

Tests must prove:
1. Company A cannot read Company B transactions
2. Company A cannot post to Company B accounts
3. Reports only include data for the active company
4. Import jobs are scoped to the selected company

---

## 8. Security Architecture

### Authentication
- Local user accounts (MVP)
- Username/email + password
- Argon2id or PBKDF2 password hashing
- Session management
- Logout

### Authorization
- Granular permissions
- Roles: Administrator, Accountant, CompanyOwner, Viewer
- Permission checks in service layer

### Audit
- `AuditLog` entity for all critical operations
- Fields: user, timestamp UTC, company, action, entity, entityId, oldValue, newValue
- Immutable audit trail

### Sensitive Data
- Companies House auth codes stored in DPAPI/Credential Manager
- Never in appsettings.json
- Never logged
- Encrypted at rest where appropriate

---

## 9. Process Isolation

External processes communicate via:
- Command line arguments
- Temporary JSON files (with cleanup)
- stdin/stdout
- Exit codes

Never:
- Share memory
- Use COM interop for Arelle
- Parse human-readable output exclusively
- Run external processes on UI thread

### Arelle Worker Protocol

```
Input:  ArelleWorker.exe --input <ixbrl-path> --output <json-path> --taxonomy <taxonomy-id>
Output: JSON file with:
{
  "isValid": bool,
  "errors": [ { "code": str, "message": str, "severity": str } ],
  "warnings": [ ... ],
  "infos": [ ... ],
  "durationMs": int,
  "arelleVersion": str
}
```

---

## 10. Backup Architecture

- ZIP-based backup format: `UKAccountsBackup_YYYYMMDD_HHMMSS.zip`
- Contents:
  - `database.sqlite` (or .sqlite3)
  - `documents/` (mirror of document store)
  - `settings.json`
  - `metadata.json` (version, schema version, company numbers)
- Restore wizard:
  1. Select backup file
  2. Verify integrity (hash, metadata)
  3. Backup current database
  4. Restore
  5. Verify

---

## 11. Installer Architecture

- WiX or Advanced Installer
- Bundle:
  - Application executables
  - .NET 10 runtime (if not present)
  - Bundled Python runtime + ArelleWorker dependencies
  - iXBRLWorker dependencies (if used)
- Install to `Program Files\UKAccounts`
- Data in `%APPDATA%\UKAccounts\`
- Documents in `%APPDATA%\UKAccounts\Documents\`
- Create start menu shortcut
- Register uninstaller

---

## 12. Testing Strategy

### Unit Tests
- Domain entities and value objects
- Accounting engine (double-entry invariants)
- Validators
- Report generators (deterministic)

### Integration Tests
- EF Core repositories with SQLite in-memory or file
- Import providers with sample files
- Arelle validation with sample iXBRL
- Companies House mock client

### Critical Test Invariants
- Every posted journal: `SUM(Debits) == SUM(Credits)`
- Company A cannot access Company B data
- Reversal entries balance correctly
- Reports are deterministic (same input → same output)

### Test Data
- Test company: `TEST COMPANY LIMITED` (company number `00000000`)
- Separate configs for Development, Test, Production
- Automatic filing disabled in Development and Test
