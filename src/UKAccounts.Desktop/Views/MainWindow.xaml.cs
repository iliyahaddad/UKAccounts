using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Desktop.Services;
using UKAccounts.Desktop.ViewModels;
using UKAccounts.Desktop.Views;

namespace UKAccounts.Desktop.Views;

public partial class MainWindow : Window
{
    public string? CompanyName { get; set; }
    public string? CompanyNumber { get; set; }

    private readonly ICompanyService _companyService;
    private readonly SessionService _sessionService;

    public MainWindow()
    {
        InitializeComponent();
        _companyService = App.Host.Services.GetRequiredService<ICompanyService>();
        _sessionService = App.Host.Services.GetRequiredService<SessionService>();
        UpdateCompanyDisplay();
    }

    private void UpdateCompanyDisplay()
    {
        if (!string.IsNullOrEmpty(CompanyName))
        {
            CurrentCompanyTextBlock.Text = $"{CompanyName} ({CompanyNumber})";
        }
        else
        {
            CurrentCompanyTextBlock.Text = "No company selected";
        }
    }

    private async void NewCompany_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new CompanyEditDialog();
        if (dialog.ShowDialog() == true)
        {
            var request = new CreateCompanyRequest
            {
                CompanyNumber = dialog.CompanyNumber,
                CompanyName = dialog.CompanyName,
                RegisteredOffice = dialog.RegisteredOffice,
                CompanyType = dialog.CompanyType,
                AccountingReferenceDate = dialog.AccountingReferenceDate,
                Currency = dialog.Currency,
                Regime = dialog.Regime
            };

            var company = await _companyService.CreateAsync(request);
            MessageBox.Show($"Company created: {company.CompanyName}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private async void OpenCompany_Click(object sender, RoutedEventArgs e)
    {
        var companies = await _companyService.GetByUserAsync(_sessionService.CurrentUserId ?? Guid.Empty);
        if (!companies.Any())
        {
            MessageBox.Show("No companies found. Please create a company first.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var companyWindow = new CompanyListWindow();
        companyWindow.Owner = this;
        if (companyWindow.ShowDialog() == true && companyWindow.SelectedCompany != null)
        {
            CompanyName = companyWindow.SelectedCompany.CompanyName;
            CompanyNumber = companyWindow.SelectedCompany.CompanyNumber;
            UpdateCompanyDisplay();
            StatusTextBlock.Text = $"Opened: {CompanyName}";
        }
    }

    private void SwitchCompany_Click(object sender, RoutedEventArgs e)
    {
        _ = OpenCompany_Click(sender, e);
    }

    private void CompanySettings_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Company settings not yet implemented.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ChartOfAccounts_Click(object sender, RoutedEventArgs e)
    {
        var view = new ChartOfAccountsView { Owner = this };
        view.ShowDialog();
    }

    private void JournalEntry_Click(object sender, RoutedEventArgs e)
    {
        var view = new JournalEntryView { Owner = this };
        view.ShowDialog();
    }

    private void Customers_Click(object sender, RoutedEventArgs e)
    {
        var view = new CustomerListView { Owner = this };
        view.ShowDialog();
    }

    private void Invoices_Click(object sender, RoutedEventArgs e)
    {
        var view = new InvoiceListView { Owner = this };
        view.ShowDialog();
    }

    private void Suppliers_Click(object sender, RoutedEventArgs e)
    {
        var view = new SupplierListView { Owner = this };
        view.ShowDialog();
    }

    private void Bills_Click(object sender, RoutedEventArgs e)
    {
        var view = new BillListView { Owner = this };
        view.ShowDialog();
    }

    private void Expenses_Click(object sender, RoutedEventArgs e)
    {
        var view = new ExpenseListView { Owner = this };
        view.ShowDialog();
    }

    private void BankAccounts_Click(object sender, RoutedEventArgs e)
    {
        var view = new BankAccountView { Owner = this };
        view.ShowDialog();
    }

    private void Reconciliation_Click(object sender, RoutedEventArgs e)
    {
        var view = new BankReconciliationView { Owner = this };
        view.ShowDialog();
    }

    private void TrialBalance_Click(object sender, RoutedEventArgs e)
    {
        var view = new TrialBalanceView { Owner = this };
        view.ShowDialog();
    }

    private void ProfitLoss_Click(object sender, RoutedEventArgs e)
    {
        var view = new ProfitLossView { Owner = this };
        view.ShowDialog();
    }

    private void BalanceSheet_Click(object sender, RoutedEventArgs e)
    {
        var view = new BalanceSheetView { Owner = this };
        view.ShowDialog();
    }

    private void GeneralLedger_Click(object sender, RoutedEventArgs e)
    {
        var view = new GeneralLedgerView { Owner = this };
        view.ShowDialog();
    }

    private void CashFlow_Click(object sender, RoutedEventArgs e)
    {
        var view = new CashFlowView { Owner = this };
        view.ShowDialog();
    }

    private void AgedReceivables_Click(object sender, RoutedEventArgs e)
    {
        var view = new AgedReceivablesView { Owner = this };
        view.ShowDialog();
    }

    private void AgedPayables_Click(object sender, RoutedEventArgs e)
    {
        var view = new AgedPayablesView { Owner = this };
        view.ShowDialog();
    }

    private void PrepareAnnualAccounts_Click(object sender, RoutedEventArgs e)
    {
        var view = new AccountsProductionView { Owner = this };
        view.ShowDialog();
    }

    private void FilingHistory_Click(object sender, RoutedEventArgs e)
    {
        var view = new FilingHistoryView { Owner = this };
        view.ShowDialog();
    }

    private void ManualFiling_Click(object sender, RoutedEventArgs e)
    {
        var view = new ManualFilingView { Owner = this };
        view.ShowDialog();
    }

    private void BackupRestore_Click(object sender, RoutedEventArgs e)
    {
        var view = new BackupRestoreView { Owner = this };
        view.ShowDialog();
    }

    private async void Logout_Click(object sender, RoutedEventArgs e)
    {
        if (_sessionService.CurrentUserId.HasValue)
        {
            await App.Host.Services.GetRequiredService<IAuthenticationService>().LogoutAsync(_sessionService.CurrentUserId.Value);
        }

        _sessionService.Clear();
        var loginWindow = App.Host.Services.GetRequiredService<LoginWindow>();
        loginWindow.Show();
        Close();
    }
}
