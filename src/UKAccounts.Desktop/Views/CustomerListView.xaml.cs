using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;

namespace UKAccounts.Desktop.Views;

public partial class CustomerListView : Window
{
    private readonly ICustomerService _customerService;

    public CustomerListView()
    {
        InitializeComponent();
        _customerService = App.Host.Services.GetRequiredService<ICustomerService>();
        Loaded += async (_, __) => await LoadCustomersAsync();
    }

    private async Task LoadCustomersAsync()
    {
        var customers = await _customerService.GetByCompanyAsync(Guid.Empty);
        CustomersDataGrid.ItemsSource = customers;
    }

    private void AddCustomer_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Add customer dialog not yet implemented.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
