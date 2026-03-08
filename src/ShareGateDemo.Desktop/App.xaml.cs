using System.Linq;
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
    private const string DefaultAzureApiUrl = "https://sharegate-demo-api--0000001.jollybeach-7acd3a8a.canadacentral.azurecontainerapps.io/";

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var endpoints = config.GetSection("ApiEndpoints")
            .GetChildren()
            .Select(child => new ApiEndpointOption(
                child["Name"] ?? "Endpoint",
                child["Url"] ?? string.Empty))
            .Where(option => !string.IsNullOrWhiteSpace(option.Url))
            .ToList();

        if (endpoints.Count == 0)
        {
            endpoints.Add(new ApiEndpointOption("Local", DefaultApiBaseUrl));
            endpoints.Add(new ApiEndpointOption("Azure", DefaultAzureApiUrl));
        }

        var apiBaseUrl = config["ApiBaseUrl"] ?? DefaultApiBaseUrl;
        if (!apiBaseUrl.EndsWith("/", StringComparison.Ordinal))
        {
            apiBaseUrl += "/";
        }

        var mainWindow = new MainWindow
        {
            DataContext = new MainViewModel(apiBaseUrl, endpoints)
        };
        mainWindow.Show();
    }
}