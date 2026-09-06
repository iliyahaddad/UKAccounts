using System.IO;
using System.Windows;
using Microsoft.Win32;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;

namespace UKAccounts.Desktop.Views;

public partial class BackupRestoreView : Window
{
    private readonly IBackupService _backupService;

    public BackupRestoreView()
    {
        InitializeComponent();
        _backupService = App.Host.Services.GetRequiredService<IBackupService>();
        Loaded += async (_, __) => await LoadBackupsAsync();
    }

    private async void CreateBackup_Click(object sender, RoutedEventArgs e)
    {
        StatusTextBlock.Text = "Creating backup...";
        StatusTextBlock.Foreground = System.Windows.Media.Brushes.Blue;

        var result = await _backupService.CreateBackupAsync();

        if (result.Success)
        {
            StatusTextBlock.Text = $"Backup created: {Path.GetFileName(result.BackupPath)} ({result.Size:N0} bytes)";
            StatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;
            await LoadBackupsAsync();
        }
        else
        {
            StatusTextBlock.Text = $"Backup failed: {result.ErrorMessage}";
            StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
        }
    }

    private async void RestoreBackup_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Backup files (*.zip)|*.zip|All files (*.*)|*.*",
            Title = "Select backup file to restore"
        };

        if (dialog.ShowDialog() == true)
        {
            var result = MessageBox.Show(
                "WARNING: Restoring a backup will overwrite your current data. A backup of your current database will be created automatically.\n\nAre you sure you want to continue?",
                "Confirm Restore",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                StatusTextBlock.Text = "Restoring backup...";
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Blue;

                var restoreResult = await _backupService.RestoreBackupAsync(dialog.FileName);

                if (restoreResult.Success)
                {
                    StatusTextBlock.Text = "Backup restored successfully. Please restart the application.";
                    StatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                    MessageBox.Show("Backup restored successfully. Please restart the application.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    StatusTextBlock.Text = $"Restore failed: {restoreResult.ErrorMessage}";
                    StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                    MessageBox.Show($"Restore failed:\n{restoreResult.ErrorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    private async Task LoadBackupsAsync()
    {
        var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UKAccounts");
        var backupFolder = Path.Combine(appDataPath, "Backups");
        var backups = await _backupService.GetAvailableBackupsAsync(backupFolder);

        var items = backups.Select(b => new
        {
            FileName = Path.GetFileName(b),
            Date = File.GetLastWriteTime(b)
        }).ToList();

        BackupsDataGrid.ItemsSource = items;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
