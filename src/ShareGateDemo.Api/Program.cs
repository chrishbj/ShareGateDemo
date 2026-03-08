using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using ShareGateDemo.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<MongoDbOptions>(builder.Configuration.GetSection("Mongo"));
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<MongoDbOptions>>().Value;
    return new MongoClient(options.ConnectionString);
});
builder.Services.AddSingleton<MigrationJobRepository>();
builder.Services.AddSingleton<JobRunner>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }))
    .WithName("Health");

app.MapGet("/api/jobs", async (MigrationJobRepository repo) =>
{
    var jobs = await repo.GetAllAsync();
    return Results.Ok(jobs.Select(j => j.ToDto()));
});

app.MapGet("/api/jobs/{id}", async (string id, MigrationJobRepository repo) =>
{
    var job = await repo.GetByIdAsync(id);
    return job is null ? Results.NotFound() : Results.Ok(job.ToDto());
});

app.MapPost("/api/jobs", async (CreateJobRequest request, MigrationJobRepository repo) =>
{
    var created = await repo.CreateAsync(request);
    return Results.Created($"/api/jobs/{created.Id}", created.ToDto());
});

app.MapPut("/api/jobs/{id}/name", async (string id, UpdateJobNameRequest request, MigrationJobRepository repo) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.BadRequest(new { error = "Name is required." });
    }

    var updated = await repo.UpdateNameAsync(id, request.Name);
    return updated is null ? Results.NotFound() : Results.Ok(updated.ToDto());
});

app.MapDelete("/api/jobs/{id}", async (string id, MigrationJobRepository repo) =>
{
    var deleted = await repo.DeleteAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
});

app.MapPost("/api/jobs/{id}/run", async (string id, JobRunner runner) =>
{
    var response = await runner.RunAsync(id);
    return response is null ? Results.NotFound() : Results.Ok(response);
});

app.Run();

sealed class MongoDbOptions
{
    public string ConnectionString { get; init; } = string.Empty;
    public string Database { get; init; } = "migrate_demo";
    public string JobsCollection { get; init; } = "jobs";
}

sealed class MigrationJobDocument
{
    [BsonId]
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public string Target { get; set; } = string.Empty;

    public JobStatus Status { get; set; } = JobStatus.Pending;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public string? Note { get; set; }
}

sealed class MigrationJobRepository
{
    private readonly IMongoCollection<MigrationJobDocument> _collection;

    public MigrationJobRepository(IMongoClient client, IOptions<MongoDbOptions> options)
    {
        var db = client.GetDatabase(options.Value.Database);
        _collection = db.GetCollection<MigrationJobDocument>(options.Value.JobsCollection);
    }

    public async Task<List<MigrationJobDocument>> GetAllAsync()
    {
        var cursor = await _collection.FindAsync(FilterDefinition<MigrationJobDocument>.Empty);
        return await cursor.ToListAsync();
    }

    public async Task<MigrationJobDocument?> GetByIdAsync(string id)
    {
        var cursor = await _collection.FindAsync(j => j.Id == id);
        return await cursor.FirstOrDefaultAsync();
    }

    public async Task<MigrationJobDocument> CreateAsync(CreateJobRequest request)
    {
        var job = new MigrationJobDocument
        {
            Id = Guid.NewGuid().ToString("n"),
            Name = request.Name,
            Source = request.Source,
            Target = request.Target,
            Note = request.Note,
            Status = JobStatus.Pending,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await _collection.InsertOneAsync(job);
        return job;
    }

    public async Task UpdateStatusAsync(string id, JobStatus status)
    {
        var update = Builders<MigrationJobDocument>.Update
            .Set(j => j.Status, status)
            .Set(j => j.UpdatedAtUtc, DateTime.UtcNow);

        await _collection.UpdateOneAsync(j => j.Id == id, update);
    }

    public async Task<MigrationJobDocument?> UpdateNameAsync(string id, string name)
    {
        var update = Builders<MigrationJobDocument>.Update
            .Set(j => j.Name, name)
            .Set(j => j.UpdatedAtUtc, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(j => j.Id == id, update);
        if (result.MatchedCount == 0)
        {
            return null;
        }

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _collection.DeleteOneAsync(j => j.Id == id);
        return result.DeletedCount > 0;
    }
}

sealed class JobRunner
{
    private static readonly TimeSpan SimulatedDuration = TimeSpan.FromSeconds(2);
    private readonly MigrationJobRepository _repo;

    public JobRunner(MigrationJobRepository repo)
    {
        _repo = repo;
    }

    public async Task<RunJobResponse?> RunAsync(string id)
    {
        var job = await _repo.GetByIdAsync(id);
        if (job is null)
        {
            return null;
        }

        await _repo.UpdateStatusAsync(id, JobStatus.Running);

        _ = Task.Run(async () =>
        {
            await Task.Delay(SimulatedDuration);
            await _repo.UpdateStatusAsync(id, JobStatus.Completed);
        });

        return new RunJobResponse(id, JobStatus.Running);
    }
}

static class MappingExtensions
{
    public static MigrationJobDto ToDto(this MigrationJobDocument job)
    {
        return new MigrationJobDto(
            job.Id,
            job.Name,
            job.Source,
            job.Target,
            job.Status,
            job.UpdatedAtUtc,
            job.Note);
    }
}
