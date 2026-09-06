using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;

namespace UKAccounts.Desktop.Views;

public partial class CompanyListWindow : Window
{
    public CompanyDto? SelectedCompany { get; private set; }

    private readonly ICompanyService _companyService;
    private List<CompanyDto> _companies = new();

    public CompanyListWindow()
    {
        InitializeComponent();
        _companyService = App.Host.Services.GetRequiredService<ICompanyService>();
        Loaded += async (_, __) => await LoadCompaniesAsync();
    }

    private async Task LoadCompaniesAsync()
    {
        var companies = await _companyService.GetByUserAsync(Guid.Empty);
        _companies = companies.ToList();
        CompaniesDataGrid.ItemsSource = _companies;
    }

    private void OpenButton_Click(object sender, RoutedEventArgs e)
    {
        if (CompaniesDataGrid.SelectedItem is CompanyDto selected)
        {
            SelectedCompany = selected;
            DialogResult = true;
            Close();
        }
        else
        {
            MessageBox.Show("Please select a company.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
