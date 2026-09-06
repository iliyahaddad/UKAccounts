@echo off
setlocal enabledelayedexpansion

set PROJECT_ROOT=%~dp0..
cd /d "%PROJECT_ROOT%"

echo.
echo ========================================
echo UK Accounts Packaging Script
echo ========================================
echo.

set BUILD_CONFIG=Release
set OUTPUT_DIR=%~dp0Output
set INSTALLER_DIR=%~dp0Installer

if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"
if not exist "%INSTALLER_DIR%" mkdir "%INSTALLER_DIR%"

echo [1/4] Building solution...
dotnet build UKAccounts.sln -c %BUILD_CONFIG% --no-restore
if errorlevel 1 (
    echo Build failed!
    exit /b 1
)

echo.
echo [2/4] Publishing desktop application...
dotnet publish src/UKAccounts.Desktop/UKAccounts.Desktop.csproj -c %BUILD_CONFIG% -o "%OUTPUT_DIR%\UKAccounts" --self-contained true -r win-x64
if errorlevel 1 (
    echo Publish failed!
    exit /b 1
)

echo.
echo [3/4] Creating portable ZIP...
set VERSION=1.0.0
set ZIP_NAME=UKAccounts_Portable_%VERSION%.zip
powershell -Command "Compress-Archive -Path '%OUTPUT_DIR%\UKAccounts\*' -DestinationPath '%INSTALLER_DIR%\%ZIP_NAME%' -Force"
if errorlevel 1 (
    echo ZIP creation failed!
    exit /b 1
)

echo.
echo [4/4] Packaging complete!
echo Portable package: %INSTALLER_DIR%\%ZIP_NAME%
echo.
echo ========================================
echo Packaging completed successfully!
echo ========================================
pause
