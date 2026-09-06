using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Reporting;
using Microsoft.Win32;

namespace UKAccounts.Desktop.Views;

public partial class GeneralLedgerView : Window
{
    private readonly IReportService _reportService;
    private GeneralLedgerReport? _report;

    public GeneralLedgerView()
    {
        InitializeComponent();
        _reportService = App.Host.Services.GetRequiredService<IReportService>();
        Loaded += async (_, __) => await LoadReportAsync();
    }

    private async Task LoadReportAsync()
    {
        var fromDate = new DateTime(DateTime.Today.Year, 1, 1);
        var toDate = DateTime.Today;
        _report = await _reportService.GetGeneralLedgerAsync(Guid.Empty, Guid.Empty, fromDate, toDate);
        LedgerDataGrid.ItemsSource = _report.Entries;
    }

    private void ExportCsv_Click(object sender, RoutedEventArgs e)
    {
        if (_report == null) return;
        var csv = CsvExporter.ExportGeneralLedger(_report);
        SaveFile("CSV files (*.csv)|*.csv", "general_ledger.csv", csv);
    }

    private void SaveFile(string filter, string defaultName, byte[] content)
    {
        var dialog = new SaveFileDialog { Filter = filter, FileName = defaultName };
        if (dialog.ShowDialog() == true)
        {
            File.WriteAllBytes(dialog.FileName, content);
            MessageBox.Show("Export completed.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
