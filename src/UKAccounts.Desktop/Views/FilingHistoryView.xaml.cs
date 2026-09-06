using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;

namespace UKAccounts.Desktop.Views;

public partial class FilingHistoryView : Window
{
    private readonly IFilingService _filingService;

    public FilingHistoryView()
    {
        InitializeComponent();
        _filingService = App.Host.Services.GetRequiredService<IFilingService>();
        Loaded += async (_, __) => await LoadFilingsAsync();
    }

    private async Task LoadFilingsAsync()
    {
        var filings = await _filingService.GetByCompanyAsync(Guid.Empty);
        FilingsDataGrid.ItemsSource = filings;
    }

    private void NewFiling_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("New filing dialog not yet implemented.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
