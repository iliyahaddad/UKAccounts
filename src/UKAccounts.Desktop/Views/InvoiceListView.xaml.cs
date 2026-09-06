using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;

namespace UKAccounts.Desktop.Views;

public partial class InvoiceListView : Window
{
    private readonly IInvoiceService _invoiceService;

    public InvoiceListView()
    {
        InitializeComponent();
        _invoiceService = App.Host.Services.GetRequiredService<IInvoiceService>();
        Loaded += async (_, __) => await LoadInvoicesAsync();
    }

    private async Task LoadInvoicesAsync()
    {
        var invoices = await _invoiceService.GetByCompanyAsync(Guid.Empty);
        InvoicesDataGrid.ItemsSource = invoices;
    }

    private void NewInvoice_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("New invoice dialog not yet implemented.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
