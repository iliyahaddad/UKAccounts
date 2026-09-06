using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Reporting;
using Microsoft.Win32;

namespace UKAccounts.Desktop.Views;

public partial class CashFlowView : Window
{
    private readonly IReportService _reportService;
    private CashFlowReport? _report;

    public CashFlowView()
    {
        InitializeComponent();
        _reportService = App.Host.Services.GetRequiredService<IReportService>();
        Loaded += async (_, __) => await LoadReportAsync();
    }

    private async Task LoadReportAsync()
    {
        var fromDate = new DateTime(DateTime.Today.Year, 1, 1);
        var toDate = DateTime.Today;
        _report = await _reportService.GetCashFlowAsync(Guid.Empty, fromDate, toDate);
        var rows = new List<object>();
        foreach (var section in _report.Sections)
        {
            foreach (var line in section.Lines)
            {
                rows.Add(new { Section = section.Title, line.Description, line.Amount });
            }
        }
        CashFlowDataGrid.ItemsSource = rows;
    }

    private void ExportPdf_Click(object sender, RoutedEventArgs e)
    {
        if (_report == null) return;
        var pdf = PdfExporter.ExportCashFlow(_report);
        SaveFile("PDF files (*.pdf)|*.pdf", "cash_flow.pdf", pdf);
    }

    private void ExportCsv_Click(object sender, RoutedEventArgs e)
    {
        if (_report == null) return;
        var csv = CsvExporter.ExportCashFlow(_report);
        SaveFile("CSV files (*.csv)|*.csv", "cash_flow.csv", csv);
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
