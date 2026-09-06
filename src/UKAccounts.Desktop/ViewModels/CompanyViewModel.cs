using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Desktop.Services;

namespace UKAccounts.Desktop.ViewModels;

public class CompanyViewModel : ObservableObject
{
    private readonly ICompanyService _companyService;
    private readonly ICompaniesHouseLookupService _lookupService;
    private readonly SessionService _sessionService;

    public ObservableCollection<CompanyDto> Companies { get; set; } = new();
    public CompanyDto? SelectedCompany { get; set; }

    public ICommand CreateCompanyCommand { get; }
    public ICommand EditCompanyCommand { get; }
    public ICommand DeleteCompanyCommand { get; }
    public ICommand SelectCompanyCommand { get; }
    public ICommand LookupCompaniesHouseCommand { get; }

    public CompanyViewModel()
    {
        _companyService = App.Host.Services.GetRequiredService<ICompanyService>();
        _lookupService = App.Host.Services.GetRequiredService<ICompaniesHouseLookupService>();
        _sessionService = App.Host.Services.GetRequiredService<SessionService>();

        CreateCompanyCommand = new RelayCommand(async _ => await CreateCompanyAsync());
        EditCompanyCommand = new RelayCommand(async _ => await EditCompanyAsync(), _ => SelectedCompany != null);
        DeleteCompanyCommand = new RelayCommand(async _ => await DeleteCompanyAsync(), _ => SelectedCompany != null);
        SelectCompanyCommand = new RelayCommand(async _ => await SelectCompanyAsync(), _ => SelectedCompany != null);
        LookupCompaniesHouseCommand = new RelayCommand(async _ => await LookupCompaniesHouseAsync());

        _ = LoadCompaniesAsync();
    }

    private async Task LoadCompaniesAsync()
    {
        if (!_sessionService.CurrentUserId.HasValue) return;

        var companies = await _companyService.GetByUserAsync(_sessionService.CurrentUserId.Value);
        Companies.Clear();
        foreach (var company in companies)
        {
            Companies.Add(company);
        }
    }

    private async Task CreateCompanyAsync()
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
                IncorporationDate = dialog.IncorporationDate,
                SicCode = dialog.SicCode,
                AccountingReferenceDate = dialog.AccountingReferenceDate,
                Currency = dialog.Currency,
                Regime = dialog.Regime
            };

            var company = await _companyService.CreateAsync(request);
            Companies.Add(company);
        }
    }

    private async Task EditCompanyAsync()
    {
        if (SelectedCompany == null) return;

        var dialog = new CompanyEditDialog
        {
            CompanyNumber = SelectedCompany.CompanyNumber,
            CompanyName = SelectedCompany.CompanyName,
            RegisteredOffice = SelectedCompany.RegisteredOffice,
            CompanyType = SelectedCompany.CompanyType,
            IncorporationDate = SelectedCompany.IncorporationDate,
            SicCode = SelectedCompany.SicCode,
            AccountingReferenceDate = SelectedCompany.AccountingReferenceDate,
            Currency = SelectedCompany.Currency,
            Regime = SelectedCompany.Regime
        };

        if (dialog.ShowDialog() == true)
        {
            var request = new CreateCompanyRequest
            {
                CompanyNumber = dialog.CompanyNumber,
                CompanyName = dialog.CompanyName,
                RegisteredOffice = dialog.RegisteredOffice,
                CompanyType = dialog.CompanyType,
                IncorporationDate = dialog.IncorporationDate,
                SicCode = dialog.SicCode,
                AccountingReferenceDate = dialog.AccountingReferenceDate,
                Currency = dialog.Currency,
                Regime = dialog.Regime
            };

            var updated = await _companyService.UpdateAsync(SelectedCompany.Id, request);
            SelectedCompany.CompanyNumber = updated.CompanyNumber;
            SelectedCompany.CompanyName = updated.CompanyName;
            SelectedCompany.RegisteredOffice = updated.RegisteredOffice;
            SelectedCompany.CompanyType = updated.CompanyType;
            SelectedCompany.IncorporationDate = updated.IncorporationDate;
            SelectedCompany.SicCode = updated.SicCode;
            SelectedCompany.AccountingReferenceDate = updated.AccountingReferenceDate;
            SelectedCompany.Currency = updated.Currency;
            SelectedCompany.Regime = updated.Regime;
            SelectedCompany.IsDormant = updated.IsDormant;
        }
    }

    private async Task DeleteCompanyAsync()
    {
        if (SelectedCompany == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete {SelectedCompany.CompanyName}?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            await _companyService.DeleteAsync(SelectedCompany.Id);
            Companies.Remove(SelectedCompany);
            SelectedCompany = null;
        }
    }

    private async Task SelectCompanyAsync()
    {
        if (SelectedCompany == null) return;

        var mainWindow = App.Host.Services.GetRequiredService<MainWindow>();
        mainWindow.CompanyName = SelectedCompany.CompanyName;
        mainWindow.CompanyNumber = SelectedCompany.CompanyNumber;
        mainWindow.Close();
    }

    private async Task LookupCompaniesHouseAsync()
    {
        var dialog = new CompaniesHouseLookupDialog();
        if (dialog.ShowDialog() == true)
        {
            var result = await _lookupService.LookupAsync(dialog.CompanyNumber);
            if (result != null && result.Success)
            {
                MessageBox.Show(
                    $"Found: {result.CompanyName}\nStatus: {result.CompanyStatus}\nType: {result.CompanyType}",
                    "Companies House Lookup",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Company not found.", "Lookup Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
