using UKAccounts.Application.DTOs;

namespace UKAccounts.Application.Interfaces;

public interface IBackupService
{
    Task<BackupResult> CreateBackupAsync(string? destinationPath = null, CancellationToken cancellationToken = default);
    Task<RestoreResult> RestoreBackupAsync(string backupPath, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetAvailableBackupsAsync(string backupFolder, CancellationToken cancellationToken = default);
}
