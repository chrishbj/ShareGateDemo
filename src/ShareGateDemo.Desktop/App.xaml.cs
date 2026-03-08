using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

    public App()
    {
        DispatcherUnhandledException += (_, args) =>
        {
            var message = $"Unhandled UI exception: {args.Exception}";
            LogAndShow(message);
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            LogAndShow($"Unhandled domain exception: {args.ExceptionObject}");
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            LogAndShow($"Unobserved task exception: {args.Exception}");
            args.SetObserved();
        };
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        try
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
            MainWindow = mainWindow;
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            LogAndShow($"Startup failure: {ex}");
            Shutdown(1);
        }
    }

    private static void LogAndShow(string message)
    {
        try
        {
            var path = Path.Combine(Path.GetTempPath(), "ShareGateDemo.Desktop.error.log");
            File.WriteAllText(path, message);
        }
        catch
        {
            // ignore logging failures
        }

        MessageBox.Show(message, "ShareGateDemo Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
