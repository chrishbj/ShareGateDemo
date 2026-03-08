using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ShareGateDemo.Shared;

namespace ShareGateDemo.Desktop.Services;

public sealed class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiClient(string baseAddress)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseAddress, UriKind.Absolute)
        };

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public async Task<IReadOnlyList<MigrationJobDto>> GetJobsAsync()
    {
        var jobs = await _httpClient.GetFromJsonAsync<List<MigrationJobDto>>("api/jobs", _jsonOptions);
        return jobs ?? [];
    }

    public async Task<MigrationJobDto> CreateJobAsync(CreateJobRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/jobs", request, _jsonOptions);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<MigrationJobDto>(_jsonOptions);
        return created ?? throw new InvalidOperationException("API returned empty job payload.");
    }

    public async Task<RunJobResponse> RunJobAsync(string id)
    {
        var response = await _httpClient.PostAsync($"api/jobs/{id}/run", null);
        response.EnsureSuccessStatusCode();
        var run = await response.Content.ReadFromJsonAsync<RunJobResponse>(_jsonOptions);
        return run ?? throw new InvalidOperationException("API returned empty run payload.");
    }
}
