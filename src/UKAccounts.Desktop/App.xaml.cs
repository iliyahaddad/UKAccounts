using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Windows;
using UKAccounts.Infrastructure;
using UKAccounts.Desktop;
using UKAccounts.Desktop.Views;
using UKAccounts.Desktop.ViewModels;

namespace UKAccounts.Desktop;

public partial class App : Application
{
    public static IHost? Host { get; private set; }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false);
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddDebug();
            })
            .ConfigureServices((context, services) =>
            {
                services.AddInfrastructure(context.Configuration);
                services.AddDesktopServices();
                services.AddScoped<MainWindow>();
                services.AddScoped<LoginWindow>();
            })
            .Build();

        await Host.StartAsync();

        var loginWindow = Host.Services.GetRequiredService<LoginWindow>();
        loginWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (Host != null)
        {
            using (Host)
            {
                await Host.StopAsync();
            }
        }
        base.OnExit(e);
    }
}
