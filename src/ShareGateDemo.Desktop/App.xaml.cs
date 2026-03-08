using System.Windows;
using Microsoft.Extensions.Configuration;
using ShareGateDemo.Desktop.ViewModels;

namespace ShareGateDemo.Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private const string DefaultApiBaseUrl = "http://localhost:5069/";

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var apiBaseUrl = config["ApiBaseUrl"] ?? DefaultApiBaseUrl;
        if (!apiBaseUrl.EndsWith("/", StringComparison.Ordinal))
        {
            apiBaseUrl += "/";
        }

        var mainWindow = new MainWindow
        {
            DataContext = new MainViewModel(apiBaseUrl)
        };
        mainWindow.Show();
    }
}