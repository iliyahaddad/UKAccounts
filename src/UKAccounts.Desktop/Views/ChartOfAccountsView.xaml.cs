using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;

namespace UKAccounts.Desktop.Views;

public partial class ChartOfAccountsView : Window
{
    private readonly IChartOfAccountsService _chartOfAccountsService;

    public ChartOfAccountsView()
    {
        InitializeComponent();
        _chartOfAccountsService = App.Host.Services.GetRequiredService<IChartOfAccountsService>();
        Loaded += async (_, __) => await LoadAccountsAsync();
    }

    private async Task LoadAccountsAsync()
    {
        var accounts = await _chartOfAccountsService.GetByCompanyAsync(Guid.Empty);
        AccountsDataGrid.ItemsSource = accounts;
    }

    private void AddAccount_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Add account dialog not yet implemented.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void EditAccount_Click(object sender, RoutedEventArgs e)
    {
        if (AccountsDataGrid.SelectedItem is AccountDto account)
        {
            MessageBox.Show($"Edit account: {account.Code} - {account.Name}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show("Please select an account to edit.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
