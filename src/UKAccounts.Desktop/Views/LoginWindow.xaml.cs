using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Desktop.Services;
using UKAccounts.Desktop.ViewModels;

namespace UKAccounts.Desktop.Views;

public partial class LoginWindow : Window
{
    private readonly IAuthenticationService _authService;
    private readonly SessionService _sessionService;

    public LoginWindow()
    {
        InitializeComponent();
        _authService = App.Host.Services.GetRequiredService<IAuthenticationService>();
        _sessionService = App.Host.Services.GetRequiredService<SessionService>();
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorTextBlock.Text = string.Empty;

        var request = new LoginRequest
        {
            Username = UsernameTextBox.Text ?? string.Empty,
            Password = PasswordBox.Password ?? string.Empty
        };

        var result = await _authService.LoginAsync(request);

        if (result.Success)
        {
            _sessionService.SetSession(result);
            var mainWindow = App.Host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
            Close();
        }
        else
        {
            ErrorTextBlock.Text = result.ErrorMessage ?? "Login failed.";
        }
    }
}
