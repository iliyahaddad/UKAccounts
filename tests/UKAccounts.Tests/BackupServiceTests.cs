using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Configuration;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Infrastructure.Services;
using Xunit;

namespace UKAccounts.Tests;

public class BackupServiceTests
{
    private IBackupService CreateService(string appDataPath)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["AppDataPath"] = appDataPath,
                ["ConnectionStrings:DefaultConnection"] = "Data Source=test.db"
            })
            .Build();

        var logger = NullLogger<BackupService>.Instance;
        return new BackupService(configuration, logger);
    }

    [Fact]
    public async Task CreateBackupAsync_ShouldCreateZipFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ukaccounts_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var service = CreateService(tempDir);
            var result = await service.CreateBackupAsync();

            Assert.True(result.Success);
            Assert.NotNull(result.BackupPath);
            Assert.EndsWith(".zip", result.BackupPath);
            Assert.True(File.Exists(result.BackupPath));
            Assert.True(result.Size > 0);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public async Task RestoreBackupAsync_ShouldRestoreFromValidBackup()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ukaccounts_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var service = CreateService(tempDir);
            var backupResult = await service.CreateBackupAsync();

            Assert.True(backupResult.Success);
            Assert.NotNull(backupResult.BackupPath);

            var restoreResult = await service.RestoreBackupAsync(backupResult.BackupPath!);
            Assert.True(restoreResult.Success);
            Assert.NotNull(restoreResult.RestoredFromPath);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public async Task RestoreBackupAsync_ShouldFailForMissingFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ukaccounts_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var service = CreateService(tempDir);
            var result = await service.RestoreBackupAsync("nonexistent_backup.zip");

            Assert.False(result.Success);
            Assert.Contains("not found", result.ErrorMessage);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
