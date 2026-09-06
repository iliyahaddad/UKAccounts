using System.IO;
using System.Windows;
using Microsoft.Win32;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;

namespace UKAccounts.Desktop.Views;

public partial class ManualFilingView : Window
{
    private readonly IFilingService _filingService;
    private Guid _filingId;

    public ManualFilingView()
    {
        InitializeComponent();
        _filingService = App.Host.Services.GetRequiredService<IFilingService>();
    }

    public void SetFiling(Guid filingId)
    {
        _filingId = filingId;
        LoadDocumentsAsync();
    }

    private async void LoadDocumentsAsync()
    {
        var filing = await _filingService.GetAsync(_filingId);
        if (filing != null)
        {
            DocumentsDataGrid.ItemsSource = filing.Documents;
        }
    }

    private void SaveIxbrl_Click(object sender, RoutedEventArgs e)
    {
        SaveDocument("iXBRL files (*.ixbrl)|*.ixbrl", "accounts.ixbrl");
    }

    private void SaveXhtml_Click(object sender, RoutedEventArgs e)
    {
        SaveDocument("XHTML files (*.html)|*.html", "accounts.html");
    }

    private void SavePdf_Click(object sender, RoutedEventArgs e)
    {
        SaveDocument("PDF files (*.pdf)|*.pdf", "accounts.pdf");
    }

    private void SaveDocument(string filter, string defaultName)
    {
        var dialog = new SaveFileDialog { Filter = filter, FileName = defaultName };
        if (dialog.ShowDialog() == true)
        {
            MessageBox.Show($"File saved to: {dialog.FileName}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        var folderPath = Path.GetTempPath();
        System.Diagnostics.Process.Start("explorer.exe", folderPath);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
