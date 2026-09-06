using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Reporting;
using Microsoft.Win32;

namespace UKAccounts.Desktop.Views;

public partial class AgedReceivablesView : Window
{
    private readonly IReportService _reportService;
    private AgedReceivablesReport? _report;

    public AgedReceivablesView()
    {
        InitializeComponent();
        _reportService = App.Host.Services.GetRequiredService<IReportService>();
        Loaded += async (_, __) => await LoadReportAsync();
    }

    private async Task LoadReportAsync()
    {
        _report = await _reportService.GetAgedReceivablesAsync(Guid.Empty, DateTime.Today);
        AgedReceivablesDataGrid.ItemsSource = _report.Lines;
    }

    private void ExportCsv_Click(object sender, RoutedEventArgs e)
    {
        if (_report == null) return;
        var csv = CsvExporter.ExportAgedReceivables(_report);
        SaveFile("CSV files (*.csv)|*.csv", "aged_receivables.csv", csv);
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
