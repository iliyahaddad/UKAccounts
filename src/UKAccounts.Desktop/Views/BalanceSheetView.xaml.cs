using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Reporting;
using Microsoft.Win32;

namespace UKAccounts.Desktop.Views;

public partial class BalanceSheetView : Window
{
    private readonly IReportService _reportService;
    private BalanceSheetReport? _report;

    public BalanceSheetView()
    {
        InitializeComponent();
        _reportService = App.Host.Services.GetRequiredService<IReportService>();
        Loaded += async (_, __) => await LoadReportAsync();
    }

    private async Task LoadReportAsync()
    {
        _report = await _reportService.GetBalanceSheetAsync(Guid.Empty, DateTime.Today);
        var rows = new List<object>();
        foreach (var section in _report.Sections)
        {
            foreach (var line in section.Lines)
            {
                rows.Add(new { Section = section.Title, line.AccountCode, line.AccountName, line.Amount });
            }
        }
        BalanceSheetDataGrid.ItemsSource = rows;
    }

    private void ExportPdf_Click(object sender, RoutedEventArgs e)
    {
        if (_report == null) return;
        var pdf = PdfExporter.ExportBalanceSheet(_report);
        SaveFile("PDF files (*.pdf)|*.pdf", "balance_sheet.pdf", pdf);
    }

    private void ExportCsv_Click(object sender, RoutedEventArgs e)
    {
        if (_report == null) return;
        var csv = CsvExporter.ExportBalanceSheet(_report);
        SaveFile("CSV files (*.csv)|*.csv", "balance_sheet.csv", csv);
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
