using System.Collections.ObjectModel;
using System.Linq;
using ShareGateDemo.Desktop.Services;
using ShareGateDemo.Shared;

namespace ShareGateDemo.Desktop.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private ApiClient _apiClient;

    private string _jobName = string.Empty;
    private string _source = string.Empty;
    private string _target = string.Empty;
    private string _note = string.Empty;
    private string _statusMessage = "Ready.";
    private MigrationJobDto? _selectedJob;
    private ApiEndpointOption? _selectedEndpoint;

    public MainViewModel(string apiBaseUrl, IReadOnlyList<ApiEndpointOption> endpoints)
    {
        _apiClient = new ApiClient(apiBaseUrl);

        Jobs = new ObservableCollection<MigrationJobDto>();
        Endpoints = new ObservableCollection<ApiEndpointOption>(endpoints);
        SelectedEndpoint = ResolveSelectedEndpoint(apiBaseUrl);

        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        CreateCommand = new AsyncRelayCommand(CreateAsync, CanCreate);
        RunCommand = new AsyncRelayCommand(RunAsync, CanRun);
        SwitchEndpointCommand = new AsyncRelayCommand(SwitchEndpointAsync, CanSwitchEndpoint);

        _ = RefreshAsync();
    }

    public ObservableCollection<MigrationJobDto> Jobs { get; }
    public ObservableCollection<ApiEndpointOption> Endpoints { get; }

    public string JobName
    {
        get => _jobName;
        set
        {
            if (SetField(ref _jobName, value))
            {
                CreateCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string Source
    {
        get => _source;
        set
        {
            if (SetField(ref _source, value))
            {
                CreateCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string Target
    {
        get => _target;
        set
        {
            if (SetField(ref _target, value))
            {
                CreateCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string Note
    {
        get => _note;
        set => SetField(ref _note, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetField(ref _statusMessage, value);
    }

    public MigrationJobDto? SelectedJob
    {
        get => _selectedJob;
        set
        {
            if (SetField(ref _selectedJob, value))
            {
                RunCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public AsyncRelayCommand RefreshCommand { get; }
    public AsyncRelayCommand CreateCommand { get; }
    public AsyncRelayCommand RunCommand { get; }
    public AsyncRelayCommand SwitchEndpointCommand { get; }

    public ApiEndpointOption? SelectedEndpoint
    {
        get => _selectedEndpoint;
        set
        {
            if (SetField(ref _selectedEndpoint, value))
            {
                SwitchEndpointCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private bool CanCreate()
    {
        return !string.IsNullOrWhiteSpace(JobName)
            && !string.IsNullOrWhiteSpace(Source)
            && !string.IsNullOrWhiteSpace(Target);
    }

    private bool CanRun() => SelectedJob is not null;
    private bool CanSwitchEndpoint() => SelectedEndpoint is not null;

    private async Task RefreshAsync()
    {
        StatusMessage = "Refreshing jobs...";
        try
        {
            var jobs = await _apiClient.GetJobsAsync();
            Jobs.Clear();
            foreach (var job in jobs.OrderByDescending(j => j.UpdatedAtUtc))
            {
                Jobs.Add(job);
            }

            StatusMessage = $"Loaded {Jobs.Count} job(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Refresh failed: {ex.Message}";
        }
    }

    private async Task CreateAsync()
    {
        StatusMessage = "Creating job...";
        try
        {
            var created = await _apiClient.CreateJobAsync(
                new CreateJobRequest(JobName, Source, Target, string.IsNullOrWhiteSpace(Note) ? null : Note));

            JobName = string.Empty;
            Source = string.Empty;
            Target = string.Empty;
            Note = string.Empty;

            await RefreshAsync();
            SelectedJob = Jobs.FirstOrDefault(j => j.Id == created.Id);
            StatusMessage = "Job created.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Create failed: {ex.Message}";
        }
    }

    private async Task RunAsync()
    {
        if (SelectedJob is null)
        {
            return;
        }

        StatusMessage = $"Running job '{SelectedJob.Name}'...";
        try
        {
            await _apiClient.RunJobAsync(SelectedJob.Id);
            await RefreshAsync();
            StatusMessage = "Job started. Status will update shortly.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Run failed: {ex.Message}";
        }
    }

    private async Task SwitchEndpointAsync()
    {
        if (SelectedEndpoint is null)
        {
            return;
        }

        _apiClient = new ApiClient(SelectedEndpoint.Url);
        StatusMessage = $"Switched to {SelectedEndpoint.Name}.";
        await RefreshAsync();
    }

    private ApiEndpointOption ResolveSelectedEndpoint(string apiBaseUrl)
    {
        var normalized = NormalizeUrl(apiBaseUrl);
        var match = Endpoints.FirstOrDefault(e => NormalizeUrl(e.Url) == normalized);
        if (match is not null)
        {
            return match;
        }

        var custom = new ApiEndpointOption("Custom", apiBaseUrl);
        Endpoints.Add(custom);
        return custom;
    }

    private static string NormalizeUrl(string url)
    {
        return url.Trim().TrimEnd('/').ToLowerInvariant();
    }
}

public sealed record ApiEndpointOption(string Name, string Url);
