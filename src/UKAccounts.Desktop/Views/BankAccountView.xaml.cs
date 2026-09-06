using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using Microsoft.Win32;

namespace UKAccounts.Desktop.Views;

public partial class BankAccountView : Window
{
    private readonly IBankAccountService _bankAccountService;

    public BankAccountView()
    {
        InitializeComponent();
        _bankAccountService = App.Host.Services.GetRequiredService<IBankAccountService>();
        Loaded += async (_, __) => await LoadBankAccountsAsync();
    }

    private async Task LoadBankAccountsAsync()
    {
        var bankAccounts = await _bankAccountService.GetByCompanyAsync(Guid.Empty);
        BankAccountsDataGrid.ItemsSource = bankAccounts;
    }

    private void AddAccount_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Add bank account dialog not yet implemented.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Import_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|Excel files (*.xlsx;*.xls)|*.xlsx;*.xls|All files (*.*)|*.*",
            Title = "Select bank statement file"
        };

        if (dialog.ShowDialog() == true)
        {
            MessageBox.Show($"Import selected: {dialog.FileName}", "Import", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
