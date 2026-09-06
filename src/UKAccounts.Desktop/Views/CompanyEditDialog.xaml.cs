using System.Windows;

namespace UKAccounts.Desktop.Views;

public partial class CompanyEditDialog : Window
{
    public string CompanyNumber
    {
        get => CompanyNumberTextBox.Text ?? string.Empty;
        set => CompanyNumberTextBox.Text = value;
    }

    public string CompanyName
    {
        get => CompanyNameTextBox.Text ?? string.Empty;
        set => CompanyNameTextBox.Text = value;
    }

    public string? RegisteredOffice
    {
        get => RegisteredOfficeTextBox.Text;
        set => RegisteredOfficeTextBox.Text = value;
    }

    public string? CompanyType
    {
        get => CompanyTypeTextBox.Text;
        set => CompanyTypeTextBox.Text = value;
    }

    public DateTime? IncorporationDate { get; set; }

    public string? SicCode { get; set; }

    public DateTime AccountingReferenceDate
    {
        get => AccountingReferenceDatePicker.SelectedDate ?? DateTime.Today;
        set => AccountingReferenceDatePicker.SelectedDate = value;
    }

    public string Currency
    {
        get => CurrencyComboBox.Text ?? "GBP";
        set => CurrencyComboBox.Text = value;
    }

    public AccountsRegime Regime
    {
        get => RegimeComboBox.SelectedIndex switch
        {
            1 => AccountsRegime.MicroEntity,
            2 => AccountsRegime.SmallCompany,
            _ => AccountsRegime.Dormant
        };
        set => RegimeComboBox.SelectedIndex = value switch
        {
            AccountsRegime.MicroEntity => 1,
            AccountsRegime.SmallCompany => 2,
            _ => 0
        };
    }

    public CompanyEditDialog()
    {
        InitializeComponent();
        AccountingReferenceDate = DateTime.Today;
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CompanyNumber) || string.IsNullOrWhiteSpace(CompanyName))
        {
            MessageBox.Show("Company Number and Company Name are required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
