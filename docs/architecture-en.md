# Architecture

## Components
- **Desktop UI (WPF)**: `ShareGateDemo.Desktop`
  - MVVM pattern (ViewModels + commands)
  - API switching via dropdown
  - `ApiClient` for REST calls
- **Web API**: `ShareGateDemo.Api`
  - Minimal APIs
  - MongoDB repository for persistence
  - Background job simulation for run
- **Shared Models**: `ShareGateDemo.Shared`
  - DTOs and request/response types
- **MongoDB**
  - Collection: `jobs`

## API Endpoints
- `GET /api/health` → `{ status: "ok" }`
- `GET /api/jobs` → list jobs
- `GET /api/jobs/{id}` → job details
- `POST /api/jobs` → create job
- `PUT /api/jobs/{id}/name` → update name
- `DELETE /api/jobs/{id}` → delete job
- `POST /api/jobs/{id}/run` → run job

## Data Model (MongoDB)
- `Id` (string, GUID without dashes)
- `Name`
- `Source`
- `Target`
- `Status` (`Pending`, `Running`, `Completed`, `Failed`)
- `UpdatedAtUtc`
- `Note`

