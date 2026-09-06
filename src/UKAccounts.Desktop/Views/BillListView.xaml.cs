using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;

namespace UKAccounts.Desktop.Views;

public partial class BillListView : Window
{
    private readonly IBillService _billService;

    public BillListView()
    {
        InitializeComponent();
        _billService = App.Host.Services.GetRequiredService<IBillService>();
        Loaded += async (_, __) => await LoadBillsAsync();
    }

    private async Task LoadBillsAsync()
    {
        var bills = await _billService.GetByCompanyAsync(Guid.Empty);
        BillsDataGrid.ItemsSource = bills;
    }

    private void NewBill_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("New bill dialog not yet implemented.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
