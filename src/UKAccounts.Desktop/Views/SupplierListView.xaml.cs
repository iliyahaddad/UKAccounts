using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;

namespace UKAccounts.Desktop.Views;

public partial class SupplierListView : Window
{
    private readonly ISupplierService _supplierService;

    public SupplierListView()
    {
        InitializeComponent();
        _supplierService = App.Host.Services.GetRequiredService<ISupplierService>();
        Loaded += async (_, __) => await LoadSuppliersAsync();
    }

    private async Task LoadSuppliersAsync()
    {
        var suppliers = await _supplierService.GetByCompanyAsync(Guid.Empty);
        SuppliersDataGrid.ItemsSource = suppliers;
    }

    private void AddSupplier_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Add supplier dialog not yet implemented.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
