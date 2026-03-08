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

### WPF API Configuration

The desktop app reads `ApiBaseUrl` from `appsettings.json` and can be overridden via env var:

```bash
# Local
setx ApiBaseUrl http://localhost:5069/

# Azure
setx ApiBaseUrl https://sharegate-demo-api--30rds66.jollybeach-7acd3a8a.canadacentral.azurecontainerapps.io/
```

## Azure (Terraform + Container Apps)

This deploys a single Container App with two containers: the API and a MongoDB sidecar.

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
