# Demo Script (3-5 minutes)

## 1. Context (30s)
- "This is a minimal migration job manager to demonstrate desktop + cloud modernization."
- "WPF client uses MVVM and a shared contracts library."
- "API runs in Azure Container Apps with a MongoDB sidecar, provisioned via Terraform."

## 2. Local Run (60s)
- "Local dev uses docker-compose for MongoDB and `dotnet run` for the API."
- "The WPF app points to the API via config (`appsettings.json` or env var)."

## 3. Live Demo (90s)
- Open WPF app.
- Create a job: Source, Target, Note.
- Show job appears in list with status `Pending`.
- Click Run: status moves to `Running` then `Completed`.
- Mention this simulates a migration pipeline orchestration loop.

## 4. Cloud Deployment (60s)
- Show Terraform folder structure and call out core resources:
  - Container App Environment
  - ACR
  - Storage (Azure Files) for MongoDB data
- Show Container App with two containers: API + MongoDB sidecar.
- Mention health endpoint `/api/health` and Swagger `/swagger`.

## 5. Wrap-Up (30s)
- "This pattern gives the feature teams a stable platform: shared contracts, CI-ready containerization, IaC for repeatable environments."
- "Next steps: add real migration workers, background queue, and CI/CD pipeline."