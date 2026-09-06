using System.Windows;

namespace UKAccounts.Desktop.Views;

public partial class CompaniesHouseLookupDialog : Window
{
    public string CompanyNumber
    {
        get => CompanyNumberTextBox.Text ?? string.Empty;
        set => CompanyNumberTextBox.Text = value;
    }

    public CompaniesHouseLookupDialog()
    {
        InitializeComponent();
    }

    private void LookupButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CompanyNumber))
        {
            MessageBox.Show("Please enter a company number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
