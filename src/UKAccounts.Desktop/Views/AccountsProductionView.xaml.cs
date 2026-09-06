using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.UK;

namespace UKAccounts.Desktop.Views;

public partial class AccountsProductionView : Window
{
    private readonly AccountsProductionService _accountsProductionService;
    private readonly IArelleValidator _arelleValidator;
    private readonly ICompanyService _companyService;
    private Guid _companyId;
    private Guid _accountingPeriodId;

    public AccountsProductionView()
    {
        InitializeComponent();
        _accountsProductionService = App.Host.Services.GetRequiredService<AccountsProductionService>();
        _arelleValidator = App.Host.Services.GetRequiredService<IArelleValidator>();
        _companyService = App.Host.Services.GetRequiredService<ICompanyService>();
    }

    public void SetCompany(Guid companyId, Guid accountingPeriodId)
    {
        _companyId = companyId;
        _accountingPeriodId = accountingPeriodId;
        LoadCompanyInfo();
    }

    private async void LoadCompanyInfo()
    {
        var company = await _companyService.GetAsync(_companyId);
        if (company != null)
        {
            CompanyTextBlock.Text = $"{company.CompanyName} ({company.CompanyNumber})";
        }

        PeriodTextBlock.Text = $"Period: {_accountingPeriodId}";
    }

    private async void Generate_Click(object sender, RoutedEventArgs e)
    {
        StatusTextBlock.Text = "Generating accounts...";
        StatusTextBlock.Foreground = System.Windows.Media.Brushes.Blue;

        var regime = RegimeComboBox.SelectedIndex switch
        {
            0 => AccountsRegime.Dormant,
            1 => AccountsRegime.MicroEntity,
            2 => AccountsRegime.SmallCompany,
            _ => AccountsRegime.SmallCompany
        };

        var request = new AccountsProductionRequest
        {
            CompanyId = _companyId,
            AccountingPeriodId = _accountingPeriodId,
            Regime = regime,
            IncludeDirectorsReport = IncludeDirectorsReportCheckBox.IsChecked ?? false,
            IncludeNotes = IncludeNotesCheckBox.IsChecked ?? false,
            PeriodStart = DateTime.Today.AddYears(-1),
            PeriodEnd = DateTime.Today
        };

        var result = await _accountsProductionService.GenerateAccountsAsync(request);

        if (result.Success)
        {
            StatusTextBlock.Text = $"Accounts generated. Validating...";
            StatusTextBlock.Foreground = System.Windows.Media.Brushes.Blue;

            var tempIxbrlPath = Path.Combine(Path.GetTempPath(), result.IxbrlPath ?? "temp.ixbrl");
            if (!string.IsNullOrEmpty(result.IxbrlContent))
            {
                await File.WriteAllTextAsync(tempIxbrlPath, result.IxbrlContent);
            }

            var validationResult = await _arelleValidator.ValidateAsync(tempIxbrlPath);

            if (validationResult.IsValid)
            {
                StatusTextBlock.Text = $"Accounts generated and validated successfully.";
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                MessageBox.Show($"Accounts generated and validated successfully!\n\nFile: {result.IxbrlPath}\nGenerated: {result.GeneratedAt:g}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                StatusTextBlock.Text = $"Validation failed: {validationResult.Errors.Count} errors";
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;

                var validationView = new ArelleValidationView();
                validationView.Owner = this;
                await validationView.ValidateAsync(tempIxbrlPath);
                validationView.ShowDialog();
            }
        }
        else
        {
            StatusTextBlock.Text = $"Error: {result.ErrorMessage}";
            StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            MessageBox.Show($"Failed to generate accounts:\n{result.ErrorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
