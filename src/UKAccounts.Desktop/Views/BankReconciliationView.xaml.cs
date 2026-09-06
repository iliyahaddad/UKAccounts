using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;

namespace UKAccounts.Desktop.Views;

public partial class BankReconciliationView : Window
{
    private readonly IBankTransactionService _bankTransactionService;

    public BankReconciliationView()
    {
        InitializeComponent();
        _bankTransactionService = App.Host.Services.GetRequiredService<IBankTransactionService>();
        Loaded += async (_, __) => await LoadTransactionsAsync();
    }

    private async Task LoadTransactionsAsync()
    {
        var transactions = await _bankTransactionService.GetUnreconciledAsync(Guid.Empty);
        TransactionsDataGrid.ItemsSource = transactions;
    }

    private async void Reconcile_Click(object sender, RoutedEventArgs e)
    {
        if (TransactionsDataGrid.SelectedItem is BankTransactionDto transaction)
        {
            var result = await _bankTransactionService.ReconcileAsync(transaction.Id, Guid.Empty);
            if (result.Success)
            {
                MessageBox.Show("Transaction reconciled.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadTransactionsAsync();
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        else
        {
            MessageBox.Show("Please select a transaction to reconcile.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private async void Unreconcile_Click(object sender, RoutedEventArgs e)
    {
        if (TransactionsDataGrid.SelectedItem is BankTransactionDto transaction)
        {
            var result = await _bankTransactionService.UnreconcileAsync(transaction.Id);
            if (result.Success)
            {
                MessageBox.Show("Transaction unreconciled.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadTransactionsAsync();
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        else
        {
            MessageBox.Show("Please select a transaction to unreconcile.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
