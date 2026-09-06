# iXBRL Architecture

## 1. Overview

iXBRL (Inline XBRL) is HTML with embedded XBRL tags. It allows human-readable financial reports to be machine-readable simultaneously. UK Companies House and HMRC require iXBRL for electronic filing.

This document describes how UKAccounts generates, validates, and reviews iXBRL documents.

## 2. iXBRL Pipeline

```
┌─────────────┐     ┌──────────────┐     ┌──────────────┐     ┌─────────────┐
│   Database  │────▶│  Accounting  │────▶│   Financial  │────▶│    iXBRL    │
│             │     │    Model     │     │  Statements  │     │  Generator  │
└─────────────┘     └──────────────┘     └──────────────┘     └──────┬──────┘
                                                                      │
                                                                      ▼
                                                              ┌──────────────┐
                                                              │  XHTML +     │
                                                              │  XBRL Tags   │
                                                              └──────┬──────┘
                                                                      │
                                                                      ▼
                                                              ┌──────────────┐
                                                              │    Arelle     │
                                                              │  Validation   │
                                                              └──────┬──────┘
                                                                      │
                                                                      ▼
                                                              ┌──────────────┐
                                                              │  Validation   │
                                                              │    Result     │
                                                              └──────────────┘
```

### Stages
1. **Database** → Accounting model (journals, accounts, transactions)
2. **Accounting Model** → Financial statements (P&L, Balance Sheet)
3. **Financial Statements** → Accounts model (UK-specific structure)
4. **Accounts Model** → UK taxonomy mapping
5. **UK Taxonomy Mapping** → XHTML + XBRL tags
6. **XHTML + XBRL** → Arelle validation
7. **Validation Result** → User review / filing

## 3. Architecture Components

### 3.1 IIxbrlGenerator Interface

```csharp
public interface IIxbrlGenerator
{
    Task<GenerationResult> GenerateAsync(
        IxbrlRequest request,
        CancellationToken cancellationToken);
}

public class IxbrlRequest
{
    public Guid CompanyId { get; set; }
    public Guid AccountingPeriodId { get; set; }
    public AccountsRegime Regime { get; set; }
    public IxbrlOutputFormat Format { get; set; }
}

public class GenerationResult
{
    public bool Success { get; set; }
    public string? OutputPath { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> Warnings { get; set; } = new();
}
```

### 3.2 Implementations

#### IxbrlReporterAdapter (Python Process)
- Invokes `ixbrl-reporter` as external process
- Input: JSON representation of accounts data
- Output: iXBRL file path
- **License consideration:** ixbrl-reporter is GPL-3.0. Only use as external process if legally acceptable.

#### ArelleBasedGenerator (Native via Arelle CLI)
- Uses Arelle's instance creation capabilities
- Less flexible for UK-specific formatting
- Safer license-wise (Apache-2.0)

#### NativeCSharpGenerator (Future)
- Pure C# iXBRL generation
- No Python dependency
- Full control over taxonomy mapping
- Preferred long-term approach

## 4. UK Taxonomy Support

### Supported Taxonomies
| Regime | Taxonomy | Filing Body |
|--------|----------|-------------|
| Dormant | UK FRS-102 Dormant | Companies House |
| Micro-entity | UK FRS-105 | Companies House |
| Small company | UK FRS-102 Reduced Disclosure | Companies House |
| Full | UK FRS-102 Full | Companies House |
| CT600 | UK CT600 + DPL | HMRC |

### Taxonomy Management
- Taxonomy packages stored locally
- Download and cache from official sources
- Version tracking
- Hash verification

## 5. iXBRL Document Structure

### Minimal Dormant Accounts iXBRL
```html
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml"
      xmlns:ix="http://www.xbrl.org/2013/inlineXBRL"
      xmlns:uk-gaap="http://www.xbrl.org/uk/gaap/core/2024-01-01">
<head>
    <title>Dormant Accounts</title>
</head>
<body>
    <h1>ABC Limited</h1>
    <p>Dormant Accounts for the year ended 31 December 2025</p>

    <ix:nonNumeric name="uk-gaap:EntityLegalForm"
                    contextRef="ctx1">Limited company</ix:nonNumeric>

    <ix:nonNumeric name="uk-gaap:NameOfEntity"
                    contextRef="ctx1">ABC Limited</ix:nonNumeric>

    <!-- Balance sheet items -->
    <ix:nonFraction name="uk-gaap:ShareCapital"
                    contextRef="ctx1" unitRef="GBP"
                    decimals="2">1000</ix:nonFraction>

    <!-- More tagged facts -->
</body>
</html>
```

### Context and Unit Definitions
```html
<!-- Context: entity + period -->
<xbrli:context id="ctx1">
    <xbrli:entity>
        <xbrli:identifier scheme="http://www.companieshouse.gov.uk/">00000000</xbrli:identifier>
    </xbrli:entity>
    <xbrli:period>
        <xbrli:startDate>2025-01-01</xbrli:startDate>
        <xbrli:endDate>2025-12-31</xbrli:endDate>
    </xbrli:period>
</xbrli:context>

<!-- Unit: GBP -->
<xbrli:unit id="GBP">
    <xbrli:measure>iso:GBP</xbrli:measure>
</xbrli:unit>
```

## 6. Generation Process

### Step 1: Load Accounting Data
- Retrieve journals, accounts, transactions for period
- Validate double-entry integrity
- Calculate financial statement values

### Step 2: Build Accounts Model
- Map accounting data to UK accounts structure
- Apply accounts regime rules
- Calculate derived values
- Determine disclosure requirements

### Step 3: Map to Taxonomy
- For each accounts concept, find corresponding XBRL concept
- Apply dimensions (aspects) where required
- Determine sign conventions
- Validate required concepts are present

### Step 4: Generate XHTML
- Create XHTML document structure
- Embed XBRL contexts and units
- Insert tagged facts
- Apply formatting

### Step 5: Post-Process
- Validate XML well-formedness
- Run Arelle validation
- Generate viewer instance (optional)

## 7. Arelle Validation Integration

### Validation Request

```csharp
public class ArelleValidationRequest
{
    public string IxbrlPath { get; set; } = string.Empty;
    public string TaxonomyId { get; set; } = "UK";
    public ValidationOptions Options { get; set; } = new();
}

public class ValidationOptions
{
    public bool CheckCalculations { get; set; } = true;
    public bool CheckDimensions { get; set; } = true;
    public bool CheckUnits { get; set; } = true;
    public bool HmrcRules { get; set; } = true;
}
```

### Validation Result

```csharp
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationMessage> Errors { get; set; } = new();
    public List<ValidationMessage> Warnings { get; set; } = new();
    public List<ValidationMessage> Infos { get; set; } = new();
    public TimeSpan Duration { get; set; }
    public string ArelleVersion { get; set; } = string.Empty;
}

public class ValidationMessage
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // Error, Warning, Info
    public string? FactReference { get; set; }
    public int? LineNumber { get; set; }
}
```

### Worker Process Protocol

```json
// Input via command line args
{
  "input": "C:\\temp\\accts.ixbrl",
  "output": "C:\\temp\\validation.json",
  "taxonomy": "UK",
  "options": {
    "checkCalculations": true,
    "hmrcRules": true
  }
}

// Output JSON
{
  "isValid": false,
  "errors": [
    {
      "code": "xbrl.5.2.1.1",
      "message": "Concept 'uk-gaap:ShareCapital' is missing required dimension",
      "severity": "Error",
      "factReference": null,
      "lineNumber": 42
    }
  ],
  "warnings": [],
  "infos": [],
  "durationMs": 3420,
  "arelleVersion": "3.38.0"
}
```

## 8. iXBRL Viewer Integration

### Purpose
Allow users to review iXBRL documents interactively before submission.

### Implementation
- Generate viewer-ready iXBRL using Arelle CLI with iXBRL Viewer plugin
- Embed viewer in WPF using `WebView2` control
- Alternatively, open in default browser

### Viewer Features
- Full text search on taxonomy labels
- View tagged fact details
- Export tables to Excel
- Navigate calculation relationships

## 9. Taxonomy Management

### Local Taxonomy Store
- Store taxonomy packages in `%APPDATA%\UKAccounts\Taxonomies\`
- Track versions and hashes
- Auto-update from official sources when possible

### Taxonomy Sources
- UK FRS-102: https://www.xbrl.org/
- UK FRS-105: https://www.xbrl.org/
- HMRC CT600: https://www.gov.uk/government/organisations/hm-revenue-customs

## 10. Document Sets

For multi-document accounts (e.g., accounts + directors' report + notes):
- Use iXBRL Document Set format
- Primary file is the main accounts
- Supporting documents are additional files
- All files tagged consistently
- Arelle supports document set validation

## 11. Error Recovery

- If generation fails, preserve input data
- Log detailed error for diagnostics
- Allow user to correct and regenerate
- Never lose accounting data due to iXBRL generation failure

## 12. Future Enhancements

- **Native C# iXBRL generator** to eliminate Python dependency
- **Real-time preview** during account preparation
- **Auto-tagging suggestions** based on account descriptions
- **XBRL-CSV / XBRL-JSON** support for data interchange
- **Custom taxonomy extensions** for company-specific concepts

## 13. ixbrl-reporter Study Notes

### What to Study
1. **Configuration architecture** — How YAML configs compose reports
2. **Taxonomy mapping patterns** — How accounts map to XBRL concepts
3. **Report templates** — Structure of balance sheet, P&L reports
4. **Context generation** — How periods and entities are defined
5. **Dimension handling** — How XBRL dimensions are applied

### What Not to Copy
1. GPL-3.0 source code
2. GnuCash-specific data access
3. Python-specific infrastructure

### What to Reimplement
1. Report-to-taxonomy mapping logic (in C#)
2. Context and unit generation
3. Dimension resolution
4. Sign convention handling
5. Disclosure requirement logic
