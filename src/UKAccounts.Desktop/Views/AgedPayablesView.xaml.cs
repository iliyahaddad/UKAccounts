using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Reporting;
using Microsoft.Win32;

namespace UKAccounts.Desktop.Views;

public partial class AgedPayablesView : Window
{
    private readonly IReportService _reportService;
    private AgedPayablesReport? _report;

    public AgedPayablesView()
    {
        InitializeComponent();
        _reportService = App.Host.Services.GetRequiredService<IReportService>();
        Loaded += async (_, __) => await LoadReportAsync();
    }

    private async Task LoadReportAsync()
    {
        _report = await _reportService.GetAgedPayablesAsync(Guid.Empty, DateTime.Today);
        AgedPayablesDataGrid.ItemsSource = _report.Lines;
    }

    private void ExportCsv_Click(object sender, RoutedEventArgs e)
    {
        if (_report == null) return;
        var csv = CsvExporter.ExportAgedPayables(_report);
        SaveFile("CSV files (*.csv)|*.csv", "aged_payables.csv", csv);
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
