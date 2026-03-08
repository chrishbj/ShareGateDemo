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

Or run API via Docker (uses host Mongo on port `27018`):
```bash
docker build -t sharegate-demo-api:local -f src/ShareGateDemo.Api/Dockerfile .
docker rm -f sharegate-demo-api-local
docker run -d --name sharegate-demo-api-local -p 5069:8080 ^
  -e ASPNETCORE_URLS=http://+:8080 ^
  -e Mongo__ConnectionString="mongodb://host.docker.internal:27018" ^
  -e Mongo__Database=sharegate_demo ^
  sharegate-demo-api:local
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
$env:ApiBaseUrl = "https://sharegate-demo-api.jollybeach-7acd3a8a.canadacentral.azurecontainerapps.io/"
```

Note: `setx` affects new terminals only. For current session use `$env:ApiBaseUrl`.

### Environment Switching (Detailed)

Goal: point the desktop app at the correct API (Local or Azure) and make sure the API you expect is the one actually serving `http://localhost:5069`.

#### Switch to Local

1. Ensure MongoDB is running locally on port `27018`:
```bash
docker compose -f infra/docker-compose.yml up -d
```

2. Start the Local API (pick one):
```bash
dotnet run --project src/ShareGateDemo.Api
```
or
```bash
docker build -t sharegate-demo-api:local -f src/ShareGateDemo.Api/Dockerfile .
docker rm -f sharegate-demo-api-local
docker run -d --name sharegate-demo-api-local -p 5069:8080 ^
  -e ASPNETCORE_URLS=http://+:8080 ^
  -e Mongo__ConnectionString="mongodb://host.docker.internal:27018" ^
  -e Mongo__Database=sharegate_demo ^
  sharegate-demo-api:local
```

3. Open the WPF app, pick **Local** in the dropdown, then click **Use Endpoint**.

4. Quick check:
```powershell
Invoke-RestMethod -Uri http://localhost:5069/api/health
```

#### Switch to Azure

1. Make sure Azure deploy is green (GitHub Actions).

2. Open the WPF app, pick **Azure** in the dropdown, then click **Use Endpoint**.

3. Quick check:
```powershell
Invoke-RestMethod -Uri https://sharegate-demo-api.jollybeach-7acd3a8a.canadacentral.azurecontainerapps.io/api/health
```

#### Troubleshooting Local 404 (Edit/Delete)

If you get `404` on `PUT /api/jobs/{id}/name` or `DELETE /api/jobs/{id}`, you may be hitting an older local API process.
Make sure only one process is listening on `5069`, and stop any old process before retesting.

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
`https://sharegate-demo-api.jollybeach-7acd3a8a.canadacentral.azurecontainerapps.io/`

## CI/CD (GitHub Actions)

Workflows live in `.github/workflows`:

- `ci.yml`: Builds the solution on Windows for every PR/push.
- `deploy-azure.yml`: Deploy to Azure on push to `main`.

### Required Secrets

Set these in GitHub repo settings:

- `AZURE_CREDENTIALS` (service principal JSON for `azure/login`)
- `TF_STATE_RG` (resource group for Terraform state)
- `TF_STATE_STORAGE` (storage account for Terraform state)
- `TF_STATE_CONTAINER` (blob container for Terraform state)
- `TF_STATE_KEY` (blob name for Terraform state)

Example deploy run:
1. Run the **deploy-azure** workflow.
2. Optional input `image_tag` (defaults to commit SHA).
