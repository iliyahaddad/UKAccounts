using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;

namespace UKAccounts.Desktop.Views;

public partial class TrialBalanceView : Window
{
    private readonly ITrialBalanceService _trialBalanceService;

    public TrialBalanceView()
    {
        InitializeComponent();
        _trialBalanceService = App.Host.Services.GetRequiredService<ITrialBalanceService>();
        Loaded += async (_, __) => await LoadTrialBalanceAsync();
    }

    private async Task LoadTrialBalanceAsync()
    {
        var trialBalance = await _trialBalanceService.GetTrialBalanceAsync(Guid.Empty, DateTime.Today);
        TrialBalanceDataGrid.ItemsSource = trialBalance;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
