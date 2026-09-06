# Repository Analysis

## 1. Arelle

**URL:** https://github.com/Arelle/Arelle  
**License:** Apache License 2.0  
**Language:** Python  
**Status:** Active, production-grade  

### Purpose
Arelle is the reference open-source XBRL/iXBRL processing platform. It provides XBRL validation, taxonomy handling, formula evaluation, and filing-program validation (including HMRC UK).

### Architecture
- Desktop GUI (Tkinter), CLI (`arelleCmdLine.py`), Python API, Web Service API (Bottle)
- Plugin architecture for extensibility
- Core modules: `ModelManager`, `ModelXbrl`, `Validate`, `DisclosureSystem`, `Cntlr`, `FileSource`
- Thread-unsafe global state; not safe for concurrent sessions in one process

### Key Capabilities
- XBRL v2.1, Dimensions, Formula, Taxonomy Packages, xBRL-JSON, xBRL-CSV, Inline XBRL v1.1
- Certified validating processor by XBRL International
- Filing programme validation: EDGAR/EFM, ESEF, **HMRC (UK)**, CIPC, FERC
- Supports HMRC validation rules out of the box

### Relevance to UKAccounts
- **Primary XBRL/iXBRL validation engine**
- **iXBRL generation helper** via CLI/Python API
- **UK/HMRC validation** via built-in disclosure systems
- Must be isolated in a dedicated worker process due to global state and thread-safety issues

### Integration Approach
- Treat as external engine
- Communication: CLI args + stdin/stdout + temp JSON files
- Preferred: `ArelleWorker.exe` (Python wrapper) launched per validation request
- Capture structured validation output via `--logFile` or Python API logging handlers

### Windows Compatibility
- Runs on Windows via Python
- Official Windows installer (`installWin64.nsi`) exists
- Docker support also available

### Reusable Components
- Arelle CLI for validation
- Python API for programmatic validation
- Disclosure system plugins for HMRC rules
- Taxonomy package handling

### Components to Avoid
- Do not fork or modify Arelle source
- Do not embed Arelle GUI
- Do not run Arelle in the main UI thread
- Do not rely on thread-safe behavior

---

## 2. Arelle iXBRL Viewer

**URL:** https://github.com/Arelle/ixbrl-viewer  
**License:** Apache License 2.0  
**Language:** Python + JavaScript  
**Status:** Active  

### Purpose
Provides an interactive web-based viewer for iXBRL reports. The Python plugin prepares iXBRL files by embedding JSON metadata and a link to a standalone JavaScript viewer (`ixbrlviewer.js`).

### Architecture
- Python Arelle plugin (`iXBRLViewerPlugin`)
- JavaScript viewer application (single file `ixbrlviewer.js`)
- Can be built locally via npm or accessed via CDN

### Relevance to UKAccounts
- Useful for **reviewing generated iXBRL** before submission
- Can be invoked from Arelle CLI with `--save-viewer`
- For desktop app, embed WebView2 to display the viewer

### Integration Approach
- Optional: generate viewer-enabled iXBRL for user review
- Use CDN or locally bundled `ixbrlviewer.js`
- Run via Arelle CLI in worker process

---

## 3. Accounts Machine Ecosystem

**URL:** https://github.com/accountsmachine  
**License:** GPL-3.0 (repos indicate GPL-3.0)  
**Language:** Python, TypeScript  
**Status:** Active but low activity  

### Repositories
- `accounts-svc` — Python accounts service
- `accounts-web` — TypeScript web frontend
- `kyc` — Companies House KYC search tool
- `vat-test-service`, `companies-test-service` — Test services

### Purpose
Open-source accounting platform aimed at UK companies. Provides web-based accounting with Companies House integration.

### Relevance to UKAccounts
- **Reference architecture** for UK accounting data model
- **Companies House lookup patterns** (`kyc` tool)
- **VAT and Companies House test services** for mock testing
- GPL-3.0 license makes direct code reuse problematic for a commercial desktop app

### Integration Approach
- Study domain model and API design
- Do not copy GPL-3.0 code directly into a proprietary/commercial product
- Reimplement concepts in clean C# domain layer
- Use test service patterns for mock development

---

## 4. Cybermaggedon Ecosystem

**License:** GPL-3.0  
**Language:** Python  
**Status:** Active (some repos archived/deprecated)  

### Repositories

#### 4.1 ixbrl-reporter
**URL:** https://github.com/cybermaggedon/ixbrl-reporter  
**License:** GPL-3.0  
**Purpose:** Automated iXBRL generation from template config + account data (GnuCash/CSV). Supports UK Companies House (FRS-102/FRS-105), HMRC CT600, ESEF.

**Key Components:**
- `ixbrl_reporter/` — core engine
- `report/` — report generation
- `taxonomy/` — taxonomy definitions
- YAML-based configuration
- Input: GnuCash SQLite/XML, CSV
- Output: iXBRL (XHTML + XBRL tags)

**Relevance:**
- Excellent reference for **iXBRL generation pipeline**
- UK taxonomy mappings and report templates
- GPL-3.0 prevents direct code inclusion in commercial product
- **Adapter pattern**: call as external Python process, receive JSON/iXBRL output

#### 4.2 ixbrl-reporter-jsonnet
**URL:** https://github.com/cybermaggedon/ixbrl-reporter-jsonnet  
**License:** GPL-3.0  
**Purpose:** Jsonnet templates for generating ixbrl-reporter configuration files. Makes complex YAML configs manageable.

**Relevance:**
- Reference for **configuration architecture**
- Example templates for UK micro-entity accounts, CT600, ESEF
- Study patterns, reimplement in C# configuration system

#### 4.3 companies-house-filing
**URL:** https://github.com/cybermaggedon/companies-house-filing  
**License:** GPL-3.0  
**Purpose:** Partial implementation of Companies House Software Filing API. Submits iXBRL accounts.

**Key Components:**
- `ch_filing/` — CH filing client
- XML-based GovTalk message construction
- Authentication via presenter ID + company auth code
- Submission, status polling

**Relevance:**
- Reference for **Companies House API integration**
- XML schema understanding
- GPL-3.0 prevents direct reuse
- Study message formats, reimplement in C#

#### 4.4 ct600
**URL:** https://github.com/cybermaggedon/ct600  
**License:** GPL-3.0  
**Purpose:** HMRC Corporation Tax (CT600) submission. Creates form values from iXBRL computations, submits via HMRC API.

**Relevance:**
- Future CT600 integration reference
- Not needed for Phase 1-11 (Companies House accounts only)
- Study for future HMRC module

#### 4.5 gnucash-uk-corptax
**URL:** https://github.com/cybermaggedon/gnucash-uk-corptax  
**License:** GPL-3.0  
**Status:** Archived, deprecated in favor of `ct600`  
**Purpose:** Historical; superseded by ct600

#### 4.6 gnucash-ixbrl
**URL:** https://github.com/cybermaggedon/gnucash-ixbrl  
**License:** GPL-3.0  
**Status:** Deprecated, superseded by ixbrl-reporter  
**Purpose:** Original iXBRL generation from GnuCash

### Licensing Risk
All cybermaggedon repositories are GPL-3.0. Direct code inclusion in a commercial desktop application would require releasing the entire application under GPL-3.0. **Do not copy source code.** Use only as:
1. Architecture reference
2. Specification reference
3. Test data reference
4. External process invocation (if needed)

---

## 5. gnucash-ch-filing

**URL:** https://github.com/accountsmachine/gnucash-ch-filing  
**Status:** 404 / not found (may have been moved or removed)

---

## 6. Summary: Reusable vs Avoid

| Component | Reuse Strategy |
|-----------|---------------|
| Arelle core | External process, CLI/Python API |
| Arelle iXBRL Viewer | Optional, CDN or local JS |
| ixbrl-reporter configs | Reference only, reimplement |
| companies-house-filing | Reference only, reimplement |
| ct600 | Future reference only |
| Accounts Machine domain | Reference only, reimplement |
| GPL-3.0 source code | DO NOT COPY |

---

## 7. Proposed Dependency Graph

```
UKAccounts.Desktop (WPF)
    |
    +-- UKAccounts.Application
    |       |
    |       +-- UKAccounts.Domain
    |       +-- UKAccounts.Accounting
    |       +-- UKAccounts.UK
    |       +-- UKAccounts.Xbrl
    |       +-- UKAccounts.CompaniesHouse
    |       +-- UKAccounts.Import
    |       +-- UKAccounts.Security
    |       +-- UKAccounts.Reporting
    |
    +-- UKAccounts.Infrastructure
    |       +-- EF Core / SQLite
    |       +-- File system
    |
    +-- UKAccounts.ArelleWorker (external process)
    |       +-- Arelle (Python, bundled)
    |
    +-- UKAccounts.IxbrlWorker (external process, optional)
    |       +-- ixbrl-reporter / Jsonnet (Python, bundled)
    |
    +-- UKAccounts.Tests
    +-- UKAccounts.IntegrationTests
```

---

## 8. Python Integration Plan

### Development
- Install Python 3.11+ alongside development environment
- Create `UKAccounts.ArelleWorker` console project (C# or Python wrapper)
- Desktop app launches worker with command line args
- Worker writes structured JSON to temp file
- Desktop app reads JSON and displays results

### Production
- Bundle Python runtime with installer
- Or use `pythonnet` / `Python.Included` to embed Python
- Or compile Arelle/ixbrl-reporter to exe with PyInstaller
- Preferred: separate worker executables for isolation

### Process Isolation Pattern
```
Desktop App
  |
  +-- Start-Process ArelleWorker.exe --input ixbrl.html --output result.json
  |
  +-- Wait for exit (with timeout + cancellation token)
  |
  +-- Read result.json
  |
  +-- Parse ValidationResult
  |
  +-- Display to user
```

---

## 9. Arelle Integration Plan

### Phase 1: Validation Only
- Create `IArelleValidator` interface
- Implement `ArelleValidator` that launches `ArelleWorker.exe`
- Worker uses Arelle CLI: `arelleCmdLine.py --validate --logFile result.json`
- Parse Arelle log output into structured `ValidationMessage` list
- Display errors/warnings/infos in UI

### Phase 2: iXBRL Inspection
- Use Arelle CLI to load and inspect iXBRL
- Extract fact values for verification
- Use iXBRL Viewer plugin for review mode

### Never
- Run Arelle in the main process
- Run multiple Arelle instances concurrently
- Assume thread safety
- Parse human-readable console output exclusively (prefer structured JSON)

---

## 10. iXBRL Generation Plan

### Phase 1: Manual Template Approach
- Study `ixbrl-reporter` YAML templates
- Create C# iXBRL generation abstractions:
  - `IIxbrlGenerator`
  - `IxbrlDocument`
  - `IxbrlFact`
- Generate minimal iXBRL for dormant accounts (simplest case)
- Embed XBRL tags in XHTML using `System.Xml`

### Phase 2: Adapter to ixbrl-reporter
- Create `IxbrlReporterAdapter` that invokes Python worker
- Input: JSON representation of accounts data
- Output: iXBRL file
- This allows leveraging existing UK taxonomy configs

### Phase 3: Native C# iXBRL
- Eventually replace Python dependency with native C# generation
- Keep `IIxbrlGenerator` interface stable

### UK Taxonomy Support
- FRS-102 (full accounts)
- FRS-105 (micro-entity)
- FRS-102 reduced disclosure (small company)
- Dormant accounts (minimal tagging)

---

## 11. Companies House Filing Plan

### Phase 1: Mock Implementation
- `MockCompaniesHouseClient` implementing `ICompaniesHouseClient`
- Simulate success, rejection, network failure, timeout, auth failure
- Full filing workflow testable without real credentials

### Phase 2: Manual Filing Package
- Generate iXBRL/XHTML/PDF
- Save to user-selected location
- Provide filing instructions
- No API submission

### Phase 3: Production API (after extensive testing)
- Implement `CompaniesHouseClient` using REST API
- Presenter account authentication
- Company authentication code
- Submission, status polling, response handling
- Full audit trail

### Credential Storage
- Use Windows DPAPI or Credential Manager
- Never store in appsettings.json
- Never log credentials

---

## 12. Database Design Principles

- **SQLite** with EF Core 10
- GUID primary keys internally
- `CompanyNumber` stored separately, indexed
- All company-scoped entities have `CompanyId`
- Decimal-compatible storage for money (`decimal(18,2)`)
- Foreign keys enabled
- Migrations for schema evolution
- Backup before migration
- Rollback capability

---

## 13. Security Design

- Password hashing: Argon2id (preferred) or PBKDF2 with strong parameters
- DPAPI / Windows Credential Manager for sensitive data
- No secrets in source code or config files
- HTTPS for all remote APIs
- Certificate validation enabled
- Rate limiting / account lockout
- Audit log for all sensitive operations
- Secure temp files (isolated, short-lived)

---

## 14. Phase-by-Phase Implementation Plan

| Phase | Focus | Deliverable |
|-------|-------|-------------|
| 0 | Research | Documentation (this file + others) |
| 1 | Solution Skeleton | Compiling VS solution with all projects |
| 2 | Authentication | Login, roles, permissions, audit log |
| 3 | Company Management | CRUD, switching, CH lookup |
| 4 | Accounting Core | Chart of accounts, journals, double-entry, trial balance |
| 5 | Invoicing & Expenses | Customers, suppliers, invoices, bills, expenses |
| 6 | Bank Import | CSV/Excel import, mapping, reconciliation |
| 7 | Reports | P&L, Balance Sheet, Trial Balance, General Ledger, Cash Flow |
| 8 | UK Accounts | Dormant/micro/small accounts generation |
| 9 | iXBRL | Adapter to ixbrl-reporter, native generator |
| 10 | Arelle | Validation worker integration |
| 11 | Companies House | Mock + production filing |
| 12 | Backup/Restore | ZIP backup, restore wizard |
| 13 | Installer | Production Windows installer |

---

## 15. Critical Decisions

1. **Arelle is external** — never fork, always worker process
2. **ixbrl-reporter is external** — adapter only, GPL-3.0 prevents embedding
3. **Domain is pure C#** — no WPF, no SQLite references
4. **CompanyId is the boundary** — enables future multi-tenancy
5. **Double-entry is invariant** — tested at every phase
6. **No cloud dependency** — MVP works fully offline
7. **No AI in MVP** — deterministic only
8. **GPL components stay external** — invoked as processes, not linked
