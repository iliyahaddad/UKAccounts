using System.IO.Compression;
using Microsoft.Extensions.Logging;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace UKAccounts.Infrastructure.Services;

public class BackupService : IBackupService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<BackupService> _logger;

    public BackupService(IConfiguration configuration, ILogger<BackupService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<BackupResult> CreateBackupAsync(string? destinationPath = null, CancellationToken cancellationToken = default)
    {
        var result = new BackupResult();

        try
        {
            var appDataPath = _configuration["AppDataPath"] 
                ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UKAccounts");
            var databasePath = _configuration.GetConnectionString("DefaultConnection") 
                ?? "Data Source=ukaccounts.db";
            var documentsPath = Path.Combine(appDataPath, "Documents");

            var backupFolder = destinationPath 
                ?? Path.Combine(appDataPath, "Backups");
            Directory.CreateDirectory(backupFolder);

            var backupFileName = $"UKAccountsBackup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zip";
            var backupPath = Path.Combine(backupFolder, backupFileName);

            _logger.LogInformation("Creating backup: {BackupPath}", backupPath);

            using var zip = new ZipArchive(File.OpenWrite(backupPath), ZipArchiveMode.Create);

            var metadata = new
            {
                version = "1.0.0",
                schemaVersion = "1.0",
                createdAt = DateTime.UtcNow,
                databaseFile = Path.GetFileName(databasePath),
                documentCount = Directory.Exists(documentsPath) ? Directory.GetFiles(documentsPath).Length : 0
            };

            var metadataJson = System.Text.Json.JsonSerializer.Serialize(metadata, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            var metadataEntry = zip.CreateEntry("metadata.json");
            using var metadataStream = metadataEntry.Open();
            using var metadataWriter = new StreamWriter(metadataStream);
            await metadataWriter.WriteAsync(metadataJson);

            if (File.Exists(databasePath))
            {
                zip.CreateEntryFromFile(databasePath, "database.sqlite");
                _logger.LogInformation("Database backed up: {DatabasePath}", databasePath);
            }

            if (Directory.Exists(documentsPath))
            {
                var files = Directory.GetFiles(documentsPath, "*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var relativePath = Path.GetRelativePath(documentsPath, file);
                    zip.CreateEntryFromFile(file, Path.Combine("Documents", relativePath));
                }
                _logger.LogInformation("Documents backed up: {Count} files", files.Length);
            }

            result.Success = true;
            result.BackupPath = backupPath;
            result.Size = new FileInfo(backupPath).Length;
            result.CreatedAt = DateTime.UtcNow;

            _logger.LogInformation("Backup created successfully: {BackupPath} ({Size} bytes)", backupPath, result.Size);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Backup failed");
            result.Success = false;
            result.ErrorMessage = $"Backup failed: {ex.Message}";
        }

        return result;
    }

    public async Task<RestoreResult> RestoreBackupAsync(string backupPath, CancellationToken cancellationToken = default)
    {
        var result = new RestoreResult();

        try
        {
            if (!File.Exists(backupPath))
            {
                result.Success = false;
                result.ErrorMessage = $"Backup file not found: {backupPath}";
                return result;
            }

            var appDataPath = _configuration["AppDataPath"] 
                ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UKAccounts");
            var databasePath = _configuration.GetConnectionString("DefaultConnection") 
                ?? "Data Source=ukaccounts.db";
            var documentsPath = Path.Combine(appDataPath, "Documents");

            _logger.LogInformation("Restoring backup: {BackupPath}", backupPath);

            string? extractedMetadata = null;
            using (var zip = ZipFile.OpenRead(backupPath))
            {
                foreach (var entry in zip.Entries)
                {
                    if (entry.FullName == "metadata.json")
                    {
                        using var stream = entry.Open();
                        using var reader = new StreamReader(stream);
                        extractedMetadata = await reader.ReadToEndAsync();
                        continue;
                    }

                    if (entry.FullName.StartsWith("database.sqlite", StringComparison.OrdinalIgnoreCase))
                    {
                        var backupDbPath = Path.Combine(appDataPath, "database_backup_before_restore.sqlite");
                        if (File.Exists(databasePath))
                        {
                            File.Copy(databasePath, backupDbPath, true);
                            _logger.LogInformation("Current database backed up to: {BackupDbPath}", backupDbPath);
                        }

                        entry.ExtractToFile(databasePath, true);
                        _logger.LogInformation("Database restored from backup");
                    }
                    else if (entry.FullName.StartsWith("Documents/", StringComparison.OrdinalIgnoreCase))
                    {
                        var relativePath = entry.FullName.Substring("Documents/".Length);
                        var targetPath = Path.Combine(documentsPath, relativePath);
                        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                        entry.ExtractToFile(targetPath, true);
                    }
                }
            }

            result.Success = true;
            result.RestoredFromPath = backupPath;
            result.RestoredAt = DateTime.UtcNow;

            _logger.LogInformation("Backup restored successfully from: {BackupPath}", backupPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Restore failed");
            result.Success = false;
            result.ErrorMessage = $"Restore failed: {ex.Message}";
        }

        return result;
    }

    public Task<IReadOnlyList<string>> GetAvailableBackupsAsync(string backupFolder, CancellationToken cancellationToken = default)
    {
        var backups = new List<string>();

        if (Directory.Exists(backupFolder))
        {
            var files = Directory.GetFiles(backupFolder, "UKAccountsBackup_*.zip");
            backups.AddRange(files.OrderByDescending(f => f).ToList());
        }

        return Task.FromResult((IReadOnlyList<string>)backups);
    }
}
