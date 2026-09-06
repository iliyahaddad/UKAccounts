using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;

namespace UKAccounts.Desktop.Views;

public partial class JournalEntryView : Window
{
    private readonly IJournalService _journalService;
    private readonly IChartOfAccountsService _chartOfAccountsService;
    private readonly IAccountingEngine _accountingEngine;

    public JournalEntryView()
    {
        InitializeComponent();
        _journalService = App.Host.Services.GetRequiredService<IJournalService>();
        _chartOfAccountsService = App.Host.Services.GetRequiredService<IChartOfAccountsService>();
        _accountingEngine = App.Host.Services.GetRequiredService<IAccountingEngine>();
    }

    private void AddLine_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Add journal line dialog not yet implemented.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void Post_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Journal posting requires a complete journal with balanced lines.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
