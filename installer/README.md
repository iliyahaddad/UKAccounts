# UK Accounts Installer

This directory contains the WiX-based installer configuration for UK Accounts.

## Files

- `UKAccounts.wxs` - Main WiX installer definition

## Building the Installer

### Prerequisites

- WiX Toolset v3.11+ (https://wixtoolset.org/releases/)
- Visual Studio 2022 or .NET SDK 8.0+

### Build Steps

1. Build the solution in Release mode:
   ```bash
   dotnet build UKAccounts.sln -c Release
   ```

2. Build the WiX installer:
   ```bash
   candle.exe installer\UKAccounts.wxs -o installer\UKAccounts.wixobj
   light.exe installer\UKAccounts.wixobj -o UKAccountsSetup.msi
   ```

## Installation

The installer creates:
- Start Menu shortcut
- Desktop shortcut
- Application files in `[ProgramFilesFolder]\UKAccounts`

## Uninstallation

Remove via Windows "Add or Remove Programs" or run:
```bash
msiexec /x {product-code}
```

## Notes

- Replace `PUT-GUID-HERE` with actual GUIDs before building
- The installer requires admin privileges (`InstallScope="perMachine"`)
