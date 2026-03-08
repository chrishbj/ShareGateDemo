# ShareGate Migrate Demo

Minimal demo for a WPF desktop app + .NET Web API + MongoDB.

## Local Run

1. Start MongoDB
```bash
docker compose -f infra/docker-compose.yml up -d
```

2. Run API
```bash
dotnet run --project src/ShareGateDemo.Api
```

3. Run WPF
```bash
dotnet run --project src/ShareGateDemo.Desktop
```

The WPF app expects the API at `http://localhost:5069`.
