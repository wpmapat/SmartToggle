# SmartToggle

A full-stack **feature flag management system** built on Azure. SmartToggle lets you turn features ON or OFF in your applications at runtime — no redeployment needed.

## Live Demo

> **[Live Demo →](https://smarttoggle-api.azurewebsites.net/demo)**  
> Toggle `dark-theme` or `large-font` flags and watch the demo page update in real time.

---

## What It Does

- Manage **companies**, **services**, and their **feature flags** through a web UI
- Each user sees only their own data — full **multi-tenant isolation**
- Services read their flags at runtime via a public API endpoint
- Flag changes take effect within **10 seconds** — no redeployment required

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend API | ASP.NET Core 8 Web API |
| Database | Azure Cosmos DB (NoSQL) |
| Authentication | Azure Entra ID — JWT bearer tokens |
| Frontend | React + TypeScript (Vite) |
| API Hosting | Azure App Service |
| UI Hosting | Azure Static Web Apps *(coming soon)* |
| CI/CD | GitHub Actions |

---

## Architecture

```
Browser (React + MSAL)
    │
    │  JWT Bearer Token (user login)
    ▼
ASP.NET Core 8 API  ──► Azure Cosmos DB
    │                        │
    │  [AllowAnonymous]       └── Companies
    ▼                        └── Services
Demo Service Page            └── FeatureFlags
(polls every 10s)
```

**Multi-tenancy:** The `oid` claim from the JWT token scopes all data queries per user. User A never sees User B's companies or flags.

---

## Data Model

```
Company
  └── Service
        └── FeatureFlag (boolean)
```

- Deleting a company is blocked if it has services
- Deleting a service is blocked if it has feature flags
- Creating a flag requires the service to exist

---

## Key Features

### Feature Flag Management
Create and manage boolean feature flags scoped to a service. Toggle them on/off instantly from the UI.

### Multi-Tenant Data Isolation
Each authenticated user sees only their own companies and services, enforced at the data layer using the Azure AD `oid` claim.

### Demo Service
A live demo page that reads its own feature flags from SmartToggle and changes its appearance in real time — demonstrating the core value of the system without any code changes or redeployment.

### Secure by Default
- All write operations require a valid Azure AD JWT token
- Managed Identity used for Cosmos DB access — no connection strings stored
- Public read-only endpoint for service flag consumption

---

## Running Locally

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- Azure Cosmos DB account (or emulator)
- Azure AD app registration

### API
```bash
cd SmartToggle
dotnet run
```

Add the following to `appsettings.Development.json` or user secrets:
```json
{
  "AzureAd": {
    "TenantId": "<your-tenant-id>",
    "ClientId": "<your-client-id>",
    "Audience": "api://<your-client-id>"
  },
  "CosmosDb": {
    "AccountEndpoint": "<your-cosmos-endpoint>",
    "DatabaseName": "SmartToggleDB",
    "CompaniesContainerName": "Companies",
    "ServicesContainerName": "Services",
    "FeatureFlagsContainerName": "FeatureFlags"
  }
}
```

### UI
```bash
cd SmartToggle.UI
npm install
npm run dev
```

Open `http://localhost:5173`

---

## Project Structure

```
SmartToggle/
├── SmartToggle/              # ASP.NET Core 8 Web API
│   ├── Controllers/          # API endpoints
│   ├── BusinessLogic/        # Business logic + repository interfaces
│   └── Models/               # Data models
├── SmartToggle.UI/           # React + TypeScript frontend
│   └── src/
│       └── pages/            # CompaniesPage, ServicesPage, FeatureFlagsPage, DemoServicePage
└── SmartToggle.Tests/        # xUnit unit tests (16 tests)
```

---

## Tests

```bash
cd SmartToggle.Tests
dotnet test
```

16 unit tests covering business logic using xUnit and Moq.
