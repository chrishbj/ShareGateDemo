# Deployment (Local + Azure)

## Local

### Start MongoDB (host port `27018`)
```bash
docker compose -f infra/docker-compose.yml up -d
```
**Purpose**: Starts local MongoDB for the API.

### Run API (choose one)
**Option A: dotnet**
```bash
dotnet run --project src/ShareGateDemo.Api
```
**Purpose**: Run API locally on `http://localhost:5069`.

**Option B: Docker**
```bash
docker build -t sharegate-demo-api:local -f src/ShareGateDemo.Api/Dockerfile .
docker rm -f sharegate-demo-api-local
docker run -d --name sharegate-demo-api-local -p 5069:8080 ^
  -e ASPNETCORE_URLS=http://+:8080 ^
  -e Mongo__ConnectionString="mongodb://host.docker.internal:27018" ^
  -e Mongo__Database=sharegate_demo ^
  sharegate-demo-api:local
```
**Purpose**: Run API in a container while reusing local MongoDB.

### Run WPF
```bash
dotnet run --project src/ShareGateDemo.Desktop
```
or
```powershell
Start-Process .\src\ShareGateDemo.Desktop\bin\Debug\net8.0-windows\ShareGateDemo.Desktop.exe
```

### Health Check
```powershell
Invoke-RestMethod -Uri http://localhost:5069/api/health
```

## Azure

### Authenticate
```bash
az login
```
**Purpose**: Authenticate Azure CLI.

### Provision Infrastructure
```bash
cd infra/terraform
terraform init
terraform apply
```
**Purpose**: Create RG, ACR, Log Analytics, Container App environment, etc.

### Build & Push API Image
```bash
az acr login -n <acr-name-from-output>
docker build -t <acr-login-server>/sharegate-demo-api:v1 -f src/ShareGateDemo.Api/Dockerfile .
docker push <acr-login-server>/sharegate-demo-api:v1
```
**Purpose**: Push API image to Azure Container Registry.

### Deploy Image
```bash
cd infra/terraform
terraform apply -var="api_image_tag=v1"
```
**Purpose**: Update Container App to use the new image tag.

### Get API URL
```bash
terraform output container_app_url
```

### Current Azure URL
```
https://sharegate-demo-api.jollybeach-7acd3a8a.canadacentral.azurecontainerapps.io/
```

## Environment Switching (Desktop)

1. Choose **Local** or **Azure** in the dropdown.
2. Click **Use Endpoint**.

Quick checks:
```powershell
# Local
Invoke-RestMethod -Uri http://localhost:5069/api/health

# Azure
Invoke-RestMethod -Uri https://sharegate-demo-api.jollybeach-7acd3a8a.canadacentral.azurecontainerapps.io/api/health
```

Troubleshooting: If edit/delete returns `404` on Local, ensure the API process on `5069` is the latest version.

