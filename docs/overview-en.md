# ShareGateDemo Overview

## What This System Does
- A WPF desktop app manages migration jobs.
- A .NET Web API provides CRUD + run operations.
- MongoDB stores job data.
- The desktop app can switch between Local and Azure APIs.

## Core Features
- Create migration jobs with `Name`, `Source`, `Target`, and `Note`.
- View and sort jobs (latest first).
- Run a job (simulated status progression).
- Edit a job name.
- Delete a job.
- Switch API endpoint (Local or Azure) from the UI.

## Solution Layout
- `src/ShareGateDemo.Desktop`: WPF UI, ViewModels, API client
- `src/ShareGateDemo.Api`: Minimal API endpoints, MongoDB repository
- `src/ShareGateDemo.Shared`: DTOs and request models shared by UI and API
- `infra/`: local Docker compose and Terraform for Azure
  - `infra/docker-compose.yml`
  - `infra/terraform/`

