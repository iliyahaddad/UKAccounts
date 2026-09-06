using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;

namespace UKAccounts.Desktop.Views;

public partial class ArelleValidationView : Window
{
    private readonly IArelleValidator _arelleValidator;

    public ArelleValidationView()
    {
        InitializeComponent();
        _arelleValidator = App.Host.Services.GetRequiredService<IArelleValidator>();
    }

    public async Task ValidateAsync(string ixbrlPath)
    {
        StatusTextBlock.Text = "Validating...";
        StatusTextBlock.Foreground = System.Windows.Media.Brushes.Blue;
        DurationTextBlock.Text = "";

        try
        {
            var result = await _arelleValidator.ValidateAsync(ixbrlPath);

            DurationTextBlock.Text = $"Duration: {result.Duration.TotalSeconds:F2}s | Arelle: {result.ArelleVersion}";

            ErrorsDataGrid.ItemsSource = result.Errors;
            WarningsDataGrid.ItemsSource = result.Warnings;
            InfosDataGrid.ItemsSource = result.Infos;

            if (result.IsValid)
            {
                StatusTextBlock.Text = "Validation passed";
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;
            }
            else
            {
                StatusTextBlock.Text = $"Validation failed: {result.Errors.Count} errors, {result.Warnings.Count} warnings";
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            }
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"Validation error: {ex.Message}";
            StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            MessageBox.Show($"Validation failed:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
