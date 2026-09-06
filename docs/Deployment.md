# Deployment Guide

This guide covers deploying UK Accounts to end users.

## Deployment Options

### Option 1: Portable ZIP (Recommended for MVP)

1. Run `packaging\build-packages.bat`
2. Distribute `UKAccounts_Portable_1.0.0.zip`
3. Users extract and run `UKAccounts.Desktop.exe`

**Pros**: No installation required, no admin rights
**Cons**: No Start Menu/Desktop shortcuts

### Option 2: MSI Installer

1. Ensure WiX Toolset is installed
2. Build the MSI using the instructions in `installer/README.md`
3. Distribute `UKAccountsSetup.msi`

**Pros**: Professional installation, shortcuts, easy uninstall
**Cons**: Requires admin rights

## System Requirements

- Windows 10 or later
- .NET 10 Runtime (included in self-contained build)
- 100MB disk space
- 2GB RAM recommended

## Post-Installation

1. Launch UK Accounts
2. Create a company file (or open existing)
3. Configure backup settings in Settings > Backup/Restore

## Updates

- Replace the application folder or run the MSI update
- User data (SQLite database, documents) is preserved

## Troubleshooting

- **Database locked**: Ensure only one instance is running
- **Permission denied**: Run as administrator or use portable mode
- **Missing files**: Re-download the package
