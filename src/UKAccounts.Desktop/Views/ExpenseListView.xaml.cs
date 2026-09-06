using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;

namespace UKAccounts.Desktop.Views;

public partial class ExpenseListView : Window
{
    private readonly IExpenseService _expenseService;

    public ExpenseListView()
    {
        InitializeComponent();
        _expenseService = App.Host.Services.GetRequiredService<IExpenseService>();
        Loaded += async (_, __) => await LoadExpensesAsync();
    }

    private async Task LoadExpensesAsync()
    {
        var expenses = await _expenseService.GetByCompanyAsync(Guid.Empty);
        ExpensesDataGrid.ItemsSource = expenses;
    }

    private void NewExpense_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("New expense dialog not yet implemented.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
