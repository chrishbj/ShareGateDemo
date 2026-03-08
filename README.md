# ShareGate Migrate Demo

Minimal demo for a WPF desktop app + .NET Web API + MongoDB.

## Local Run

1. Start MongoDB (mapped to host port `27018`)
```bash
docker compose -f infra/docker-compose.yml up -d
```

2. Run API
```bash
dotnet run --project src/ShareGateDemo.Api
```

3. Run WPF (either option)
```bash
dotnet run --project src/ShareGateDemo.Desktop
```
```powershell
Start-Process .\src\ShareGateDemo.Desktop\bin\Debug\net8.0-windows\ShareGateDemo.Desktop.exe
```

Local API default: `http://localhost:5069/`

## WPF Endpoint Switching

The desktop app loads endpoints from `src/ShareGateDemo.Desktop/appsettings.json` and exposes a dropdown in the UI.
Click **Use Endpoint** to switch between Local and Azure.

You can also override the API at startup with an environment variable:
```powershell
# Local
$env:ApiBaseUrl = "http://localhost:5069/"

# Azure
$env:ApiBaseUrl = "https://sharegate-demo-api--0000001.jollybeach-7acd3a8a.canadacentral.azurecontainerapps.io/"
```

Note: `setx` affects new terminals only. For current session use `$env:ApiBaseUrl`.

## Azure (Terraform + Container Apps)

This deploys a single Container App with two containers: the API and a MongoDB sidecar.
MongoDB uses an `EmptyDir` volume for the demo, so data is not persisted across revisions.

1. Authenticate
```bash
az login
```

2. Provision infrastructure
```bash
cd infra/terraform
terraform init
terraform apply
```

3. Build and push the API image
```bash
az acr login -n <acr-name-from-output>
docker build -t <acr-login-server>/sharegate-demo-api:v1 -f src/ShareGateDemo.Api/Dockerfile .
docker push <acr-login-server>/sharegate-demo-api:v1
```

4. Deploy the API image
```bash
cd infra/terraform
terraform apply -var="api_image_tag=v1"
```

5. Get the API URL
```bash
terraform output container_app_url
```

Current Azure API URL:
`https://sharegate-demo-api--0000001.jollybeach-7acd3a8a.canadacentral.azurecontainerapps.io/`