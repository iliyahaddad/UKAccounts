# Dependencies

## 1. Core Framework

| Dependency | Version | Purpose | License |
|------------|---------|---------|---------|
| .NET 10 LTS | 10.0.x | Runtime and SDK | MIT |
| Microsoft.Extensions.DependencyInjection | 10.x | DI container | MIT |
| Microsoft.Extensions.Logging | 10.x | Structured logging | MIT |
| Microsoft.Extensions.Configuration | 10.x | Configuration | MIT |

## 2. Data Access

| Dependency | Version | Purpose | License |
|------------|---------|---------|---------|
| Microsoft.EntityFrameworkCore | 10.x | ORM | MIT |
| Microsoft.EntityFrameworkCore.Sqlite | 10.x | SQLite provider | MIT |
| Microsoft.EntityFrameworkCore.Tools | 10.x | CLI tools / migrations | MIT |

## 3. UI

| Dependency | Version | Purpose | License |
|------------|---------|---------|---------|
| WPF | .NET 10 | Desktop UI framework | MIT |
| Microsoft.Xaml.Behaviors.Wpf | 1.x | MVVM behaviors | MIT |
| CommunityToolkit.Mvvm | 8.x | MVVM helpers | MIT |

## 4. Import and File Processing

| Dependency | Version | Purpose | License |
|------------|---------|---------|---------|
| CsvHelper | 33.x | CSV parsing | MS-PL |
| ClosedXML | 1.x | Excel XLSX reading | MIT |
| iTextSharp / iText7 | 7.x | PDF reading (if needed) | AGPL (use with caution) |
| PdfPig | 0.x | PDF text extraction | MIT |

**Note:** PDF libraries must be evaluated for license compatibility. iText7 is AGPL; avoid unless open-sourcing under AGPL. Prefer MIT-licensed alternatives like PdfPig.

## 5. Import and Mapping

| Dependency | Version | Purpose | License |
|------------|---------|---------|---------|
| FluentValidation | 11.x | Validation | Apache-2.0 |
| AutoMapper | 13.x | Object mapping | MIT |

## 6. Security

| Dependency | Version | Purpose | License |
|------------|---------|---------|---------|
| Sodium.Core | 2.x | Argon2id / cryptographic primitives | MIT |
| Microsoft.Windows.Compatibility | 10.x | DPAPI / Windows integration | MIT |
| System.Security.Cryptography.ProtectedData | — | DPAPI (built-in) | MIT |

## 7. Reporting and Export

| Dependency | Version | Purpose | License |
|------------|---------|---------|---------|
| QuestPDF | 2024.x | PDF generation | MIT |
| CsvHelper | 33.x | CSV export | MS-PL |

## 8. Python Integration

| Dependency | Version | Purpose | License |
|------------|---------|---------|---------|
| Arelle | latest | XBRL/iXBRL validation | Apache-2.0 |
| ixbrl-reporter | latest | iXBRL generation (optional) | GPL-3.0 |
| Python.Runtime (pythonnet) | 3.x | .NET ↔ Python bridge | MIT |

**Python Runtime:**
- Bundled with installer (Python 3.11+)
- Or PyInstaller-compiled worker executables
- No requirement for user to install Python separately

## 9. Testing

| Dependency | Version | Purpose | License |
|------------|---------|---------|---------|
| xUnit | 2.x | Test framework | MIT |
| FluentAssertions | 7.x | Test assertions | Apache-2.0 |
| Moq | 4.x | Mocking | MIT |
| Microsoft.Data.Sqlite | 10.x | In-memory SQLite for tests | MIT |
| Testcontainers | — | Optional integration test containers | MIT |

## 10. Installer

| Dependency | Version | Purpose | License |
|------------|---------|---------|---------|
| WiX Toolset | 4.x | Windows installer creation | MIT |
| Or Advanced Installer | — | Commercial installer builder | Commercial |

## 11. Not Used (Avoid)

- Entity Framework Core 6/7 (use EF Core 10)
- Newtonsoft.Json (use System.Text.Json)
- NLog / log4net (use Microsoft.Extensions.Logging)
- ASP.NET Core (not needed for desktop, keep for future web)
- Any GPL-licensed library directly linked (must remain external process)

---

## 12. Dependency Principles

1. **Domain has zero dependencies** beyond `System` and `System.Collections.Generic`
2. **Application depends on Domain only**
3. **Infrastructure depends on Application and Domain**
4. **Desktop depends on Application and Infrastructure**
5. **External engines (Arelle, ixbrl-reporter) are never referenced as libraries** — always invoked as external processes
6. **GPL-licensed code is never compiled into the application** — always external process invocation
