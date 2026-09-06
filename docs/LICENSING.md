# Licensing

## 1. UKAccounts Application License

**Proposed License:** Proprietary / Commercial

The UKAccounts desktop application is a commercial product. All original code written for this project is proprietary and not released under an open-source license.

## 2. Third-Party Licenses

### 2.1 .NET Runtime and Libraries

| Component | License | Restrictions |
|-----------|---------|--------------|
| .NET 10 LTS | MIT | None. Can be freely redistributed. |
| Microsoft.Extensions.* | MIT | None. |
| Entity Framework Core | MIT | None. |
| WPF | MIT | None. |

### 2.2 Arelle

| Component | License | Restrictions |
|-----------|---------|--------------|
| Arelle | Apache License 2.0 | Must include license and notice. Can be used commercially. Modifications must be documented. |

**Usage:** Arelle is invoked as an external process. It is bundled with the installer. The Apache-2.0 license file and any applicable NOTICE files must be included in the installer's Third-Party Licenses documentation.

### 2.3 Arelle iXBRL Viewer

| Component | License | Restrictions |
|-----------|---------|--------------|
| Arelle iXBRL Viewer | Apache License 2.0 | Same as Arelle. |

**Usage:** JavaScript viewer is either loaded from CDN or bundled locally. Attribution required.

### 2.4 ixbrl-reporter

| Component | License | Restrictions |
|-----------|---------|--------------|
| ixbrl-reporter | GPL-3.0 | **Copyleft.** If linked or distributed as part of the application, the entire application must be released under GPL-3.0. |

**Critical:** ixbrl-reporter is GPL-3.0. **Do not compile it into the application or distribute it as a library dependency.** It must be invoked as an external process only, and its source code must not be combined with proprietary code in a way that triggers copyleft.

**Safer approach:** Do not distribute ixbrl-reporter at all. Use Arelle for iXBRL generation/validation, or implement a native C# iXBRL generator.

### 2.5 ixbrl-reporter-jsonnet

| Component | License | Restrictions |
|-----------|---------|--------------|
| ixbrl-reporter-jsonnet | GPL-3.0 | Same copyleft restrictions as ixbrl-reporter. |

**Usage:** Reference for configuration architecture only. Do not include Jsonnet templates or generated configs that would trigger GPL copyleft. Reimplement configuration concepts in C#.

### 2.6 Cybermaggedon Filing Tools

| Component | License | Restrictions |
|-----------|---------|--------------|
| companies-house-filing | GPL-3.0 | Copyleft. External process only, if at all. |
| ct600 | GPL-3.0 | Copyleft. External process only, if at all. |
| gnucash-uk-corptax | GPL-3.0 | Copyleft. Deprecated. |
| gnucash-ixbrl | GPL-3.0 | Copyleft. Deprecated. |

**Usage:** Study API patterns and message formats. Reimplement in C#. Do not distribute GPL code.

### 2.7 Accounts Machine Ecosystem

| Component | License | Restrictions |
|-----------|---------|--------------|
| accounts-svc | GPL-3.0 | Copyleft. Reference only. |
| accounts-web | GPL-3.0 | Copyleft. Reference only. |
| kyc | Unknown (no license file found) | Treat as all rights reserved. |

**Usage:** Reference architecture only. Do not copy code.

### 2.8 MIT-Licensed Libraries

| Component | License | Restrictions |
|-----------|---------|--------------|
| CsvHelper | MS-PL | File must be included if modified. |
| ClosedXML | MIT | None. |
| QuestPDF | MIT | None. |
| FluentValidation | Apache-2.0 | None. |
| AutoMapper | MIT | None. |
| Moq | MIT | None. |
| xUnit | MIT | None. |
| FluentAssertions | Apache-2.0 | None. |
| CommunityToolkit.Mvvm | MIT | None. |

## 3. License Compliance Checklist

### Before Distribution
- [ ] Include `THIRD_PARTY_LICENSES.md` in installer
- [ ] Include Apache-2.0 license for Arelle
- [ ] Include Apache-2.0 license for Arelle iXBRL Viewer
- [ ] Verify no GPL code is compiled into the application
- [ ] Verify no GPL code is distributed as a library
- [ ] Include MIT/MS-PL/Apache-2.0 notices for all included libraries
- [ ] Do not distribute Python source code of GPL tools
- [ ] If using iText7, comply with AGPL (prefer PdfPig instead)

### During Development
- [ ] Do not copy GPL source into proprietary modules
- [ ] Do not link GPL libraries statically
- [ ] Do not create derivative works of GPL code
- [ ] External process invocation is the only safe integration pattern for GPL code

## 4. License Compatibility Matrix

| Use Case | Allowed? | Notes |
|----------|----------|-------|
| Call Arelle via CLI | Yes | Apache-2.0 permits this. |
| Bundle Arelle Python | Yes | Apache-2.0 permits distribution. |
| Call ixbrl-reporter via CLI | Legally risky | GPL-3.0 copyleft may apply to combined work. |
| Bundle ixbrl-reporter Python | No | Would trigger GPL-3.0 for the entire application. |
| Study ixbrl-reporter configs | Yes | Ideas are not copyrightable. |
| Reimplement ixbrl-reporter concepts in C# | Yes | Clean room implementation. |
| Use CsvHelper | Yes | MS-PL is permissive. |
| Use QuestPDF | Yes | MIT. |
| Use ClosedXML | Yes | MIT. |
| Use PdfPig | Yes | MIT. Preferred over iText7. |
| Use iText7 | Risky | AGPL requires source disclosure. Avoid. |

## 5. Recommendations

1. **Do not distribute ixbrl-reporter or any GPL-3.0 code** with the application.
2. **Use Arelle exclusively** for XBRL/iXBRL processing. It is Apache-2.0 and safe to bundle.
3. **Implement native C# iXBRL generation** rather than depending on Python GPL tools.
4. **Document all third-party components** in `THIRD_PARTY_LICENSES.md` before release.
5. **Consult a lawyer** before distribution if uncertain about any license.
