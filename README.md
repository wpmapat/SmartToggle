# SmartToggle

A full-stack **feature flag management system** built on Azure. SmartToggle lets you turn features ON or OFF in your applications at runtime — no redeployment needed.

## Live Demo

> **[SmartToggle UI →](https://white-desert-0cc31181e.7.azurestaticapps.net)**  
> Sign in with Microsoft to manage your feature flags.

> **[Live Demo →](https://smarttoggle-demoservice.azurewebsites.net)**  
> Toggle `dark-theme` or `large-font` flags in SmartToggle and watch the demo page update in real time.

---

## What It Does

- Manage **companies**, **services**, and their **feature flags** through a web UI
- Full **multi-tenant isolation** — each Azure AD tenant sees only their own data
- Client applications read their flags at runtime via a secure API
- Flag changes take effect within **10 seconds** — no redeployment required
- Cross-tenant B2B authentication — a customer app in Tenant B can securely call the API hosted in Tenant A

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend API | ASP.NET Core 8 Web API |
| Database | Azure Cosmos DB (NoSQL) |
| Authentication | Azure Entra ID — JWT bearer tokens, OAuth2 client credentials |
| Frontend | React + TypeScript (Vite) |
| Demo Service | ASP.NET Core 8 minimal API |
| API Hosting | Azure App Service |
| CI/CD | GitHub Actions |

---

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│  Tenant A (SmartToggle Operator)                         │
│                                                          │
│   Browser (React + MSAL)                                 │
│       │  JWT Bearer Token (delegated user token)         │
│       ▼                                                  │
│   ASP.NET Core 8 API  ──► Azure Cosmos DB                │
│       │                       ├── Companies              │
│       │                       ├── Services               │
│       │                       └── FeatureFlags           │
└───────┼─────────────────────────────────────────────────┘
        │
        │  OAuth2 Client Credentials (app token)
        │  appid claim = Service ID
        │  tid claim   = Company ID
        │
┌───────┼─────────────────────────────────────────────────┐
│  Tenant B (Customer — e.g. Starbucks)                    │
│                                                          │
│   SmartToggle.DemoService                                │
│       │  Authenticates using Azure AD app registration   │
│       │  in Tenant B — no user login required            │
│       ▼                                                  │
│   Demo Web Page (polls every 10s)                        │
│   UI changes based on active feature flags               │
└─────────────────────────────────────────────────────────┘
```

**Multi-tenancy:** The `tid` claim from the JWT token scopes all data queries per tenant. Tenant A never sees Tenant B's data.

**Service identity:** A client app's Azure AD `appid` claim is used as the service ID. No service IDs need to be hardcoded or configured — the token identifies the service automatically.

---

## Data Model

```
Company (ID = Azure AD Tenant ID)
  └── Service (ID = Azure AD App Client ID of the client app)
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
Each authenticated tenant sees only their own companies and services, enforced at the data layer using the Azure AD `tid` claim as the company ID.

### Cross-Tenant B2B Authentication
The demo service (Tenant B) authenticates to the SmartToggle API (Tenant A) using OAuth2 client credentials flow. The API identifies the calling service using the `appid` and `tid` claims from the token — no service IDs need to be passed in the request.

### Authorization Policy
The `ReadFeatureFlags` policy accepts both:
- **Delegated tokens** (interactive user login via the React UI)
- **App tokens** with `FeatureFlags.Read` role (machine-to-machine via client credentials)

### Demo Service
A live ASP.NET Core app deployed as a separate Azure App Service. It authenticates as a Tenant B application and reads feature flags from SmartToggle — demonstrating the core value of the system. The page updates automatically every 10 seconds without any redeployment.

### Secure by Default
- All write operations require a valid Azure AD JWT token
- Managed Identity used for Cosmos DB access — no connection strings stored
- Flag reads require either a user token or an app token with `FeatureFlags.Read` role

---

## Running Locally

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- Azure Cosmos DB account (or emulator)
- Azure AD app registration (multi-tenant)

### API
```bash
cd SmartToggle
dotnet run
```

Add the following to `appsettings.Development.json` or user secrets:
```json
{
  "AzureAd": {
    "TenantId": "organizations",
    "ClientId": "<your-client-id>",
    "Audience": "api://<your-client-id>",
    "ValidateIssuer": false
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

### Demo Service
```bash
cd SmartToggle.DemoService
dotnet run
```

Set the following environment variables or user secrets:
```json
{
  "AzureAd": {
    "TenantId": "<tenant-b-id>",
    "ClientId": "<demo-service-client-id>",
    "ClientSecret": "<demo-service-client-secret>"
  },
  "SmartToggleApi": {
    "BaseUrl": "https://localhost:7001",
    "ClientId": "<smarttoggle-api-client-id>"
  }
}
```

---

## Project Structure

```
SmartToggle/
├── SmartToggle/              # ASP.NET Core 8 Web API
│   ├── Controllers/          # API endpoints
│   ├── BusinessLogic/        # Business logic + repository interfaces
│   └── Models/               # Data models
├── SmartToggle.DemoService/  # Cross-tenant demo app (ASP.NET Core minimal API)
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
