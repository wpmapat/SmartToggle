# SmartToggle — Design Specification

---

## Product Goal

Engineering teams need to ship features safely without the risk of a full redeployment. SmartToggle lets you turn features ON or OFF in your applications at runtime — no redeployment needed.

SmartToggle solves this by:
- **Providing a control panel** to manage feature flags across multiple services
- **Enabling instant flag changes** that take effect within 10 seconds
- **Supporting cross-tenant B2B authentication** — a customer app in Tenant B can securely read its own flags from the API hosted in Tenant A
- **Isolating data per tenant** — each company sees only their own services and flags

**Target user:** Engineering teams who want to decouple feature releases from code deployments.

**Success metric:** A flag change made in the UI is reflected in the client application within 10 seconds — no redeployment required.

**Differentiation:** SmartToggle is a self-hosted, Azure-native feature flag system with full multi-tenant isolation and cross-tenant B2B authentication. Unlike third-party tools like LaunchDarkly, SmartToggle runs entirely within your Azure environment — no data leaves your tenant.

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

## Component Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│  Browser (Tenant A)                                              │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  React + TypeScript (Vite)                               │   │
│  │                                                          │   │
│  │  ┌──────────────┐  ┌──────────────┐  ┌───────────────┐  │   │
│  │  │  Companies   │  │   Services   │  │ FeatureFlags  │  │   │
│  │  │    Page      │  │    Page      │  │    Page       │  │   │
│  │  └──────────────┘  └──────────────┘  └───────────────┘  │   │
│  │                                                          │   │
│  │  MSAL — Azure AD authentication (JWT bearer token)       │   │
│  └──────────────────────────┬───────────────────────────────┘   │
└─────────────────────────────┼───────────────────────────────────┘
                              │ HTTPS + JWT
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  ASP.NET Core 8 API (Azure App Service — Tenant A)               │
│                                                                  │
│  ┌───────────────┐  ┌─────────────────┐  ┌──────────────────┐  │
│  │  Company      │  │    Service      │  │  FeatureFlag     │  │
│  │  Controller   │  │   Controller    │  │   Controller     │  │
│  └───────┬───────┘  └────────┬────────┘  └────────┬─────────┘  │
│          │                   │                    │             │
│  ┌───────▼───────┐  ┌────────▼────────┐  ┌────────▼─────────┐  │
│  │  Company      │  │    Service      │  │  FeatureFlag     │  │
│  │ BusinessLogic │  │  BusinessLogic  │  │  BusinessLogic   │  │
│  └───────┬───────┘  └────────┬────────┘  └────────┬─────────┘  │
│          │                   │                    │             │
│  ┌───────▼───────┐  ┌────────▼────────┐  ┌────────▼─────────┐  │
│  │  Company      │  │    Service      │  │  FeatureFlag     │  │
│  │  Repository   │  │   Repository    │  │   Repository     │  │
│  └───────┬───────┘  └────────┬────────┘  └────────┬─────────┘  │
└──────────┼───────────────────┼────────────────────┼─────────────┘
           │                   │                    │
           └───────────────────┴────────────────────┘
                                       │
                               ┌───────▼────────┐
                               │  Azure Cosmos  │
                               │      DB        │
                               │                │
                               │ - Companies    │
                               │ - Services     │
                               │ - FeatureFlags │
                               └────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  SmartToggle.DemoService (Azure App Service — Tenant B)          │
│                                                                  │
│  ConfidentialClientApplication (MSAL)                           │
│       │  OAuth2 Client Credentials Flow                         │
│       │  Scope: api://{smartToggleClientId}/.default            │
│       ▼                                                          │
│  HTTP GET /api/featureflag/my-flags                             │
│       │  Bearer token with appid + tid claims                   │
│       ▼                                                          │
│  Demo Web Page (polls every 10 seconds)                         │
└─────────────────────────────────────────────────────────────────┘
```

---

## How Components Talk to Each Other

**Browser → API**
- React makes HTTPS calls to the ASP.NET Core API
- Every request includes a JWT bearer token: `Authorization: Bearer <token>`
- Token obtained from Azure AD via MSAL (loginRedirect flow)
- API validates the token using Microsoft.Identity.Web

**API → Cosmos DB**
- API uses the Cosmos DB .NET SDK to read/write data
- No connection string stored — uses Managed Identity
- Every query scoped by `tid` claim (tenant ID = company ID) for data isolation

**DemoService → API (Cross-Tenant)**
- DemoService uses MSAL ConfidentialClientApplication to acquire an app token
- Token contains `appid` (service ID) and `tid` (company ID) claims
- API reads these claims to identify which service is calling — no hardcoded service IDs
- Secured by `ReadFeatureFlags` authorization policy

**Azure AD → Browser + API + DemoService**
- Browser uses MSAL to get a delegated user token
- DemoService uses client credentials to get an app token
- API uses Microsoft.Identity.Web to validate both token types
- `ValidateIssuer = false` allows cross-tenant tokens from Tenant B

```
Browser (Tenant A) → (delegated JWT) → API → Cosmos DB
DemoService (Tenant B) → (app JWT, client credentials) → API → Cosmos DB
Managed Identity → Cosmos DB (no passwords stored)
```

---

## User Journey Flowchart

```
┌─────────────┐
│  Visit App  │
└──────┬──────┘
       │
       ▼
┌─────────────────┐     Already signed in
│  Sign in with   │─────────────────────────┐
│   Microsoft     │                         │
└──────┬──────────┘                         │
       │ Auto-provision company             │
       ▼                                    ▼
┌─────────────────────────────────────────────┐
│  Companies Page                              │
│  (Company auto-created from Azure AD tenant) │
└──────────────────┬──────────────────────────┘
                   │ Click company
                   ▼
┌─────────────────────────────────────────────┐
│  Services Page                               │
│  Add service (optionally set Service ID      │
│  = Azure AD app client ID of client app)     │
└──────────────────┬──────────────────────────┘
                   │ Click service
                   ▼
┌─────────────────────────────────────────────┐
│  Feature Flags Page                          │
│  Create flags (e.g. dark-theme, large-font)  │
│  Toggle ON / OFF instantly                   │
└──────────────────┬──────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────┐
│  Client App (e.g. DemoService in Tenant B)   │
│  Polls /api/featureflag/my-flags every 10s   │
│  UI updates automatically — no redeployment  │
└─────────────────────────────────────────────┘
```

---

## Data Models

### Company
| Field | Type | Notes |
|-------|------|-------|
| Id | string | Azure AD `tid` claim (tenant ID) |
| Name | string | Organisation name |
| OwnerId | string | Azure AD `tid` claim |
| Services | List\<Service\> | Navigation property |

### Service
| Field | Type | Notes |
|-------|------|-------|
| Id | string | Azure AD `appid` of client app (or auto-generated GUID) |
| CompanyId | string | Partition key — links to Company |
| ServiceName | string | |
| Description | string | |
| Owners | List\<string\> | |

### FeatureFlag
| Field | Type | Notes |
|-------|------|-------|
| Id | string | |
| CompanyId | string | |
| ServiceId | string | Partition key |
| FlagId | string | Human-readable name e.g. `dark-theme` |
| Type | string | `bool` |
| DefaultValue | T (generic) | Current value — true or false |

---

## API Endpoints

### Company
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/company/provision` | Auto-provision company from Azure AD tenant on first login |
| GET | `/api/company` | Get all companies for current tenant |
| GET | `/api/company/{id}` | Get company by ID |
| POST | `/api/company` | Create company |
| PUT | `/api/company/{id}` | Update company |
| DELETE | `/api/company/{id}` | Delete company (blocked if services exist) |

### Service
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/service` | Get all services |
| GET | `/api/service/{id}` | Get service by ID |
| GET | `/api/service/company/{companyId}` | Get services by company |
| POST | `/api/service` | Create service |
| PUT | `/api/service/{id}` | Update service |
| DELETE | `/api/service/{id}` | Delete service (blocked if flags exist) |

### Feature Flag
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/featureflag` | Get all flags |
| GET | `/api/featureflag/{id}` | Get flag by ID |
| GET | `/api/featureflag/service/{serviceId}` | Get flags by service |
| GET | `/api/featureflag/my-flags` | Get flags for calling app (uses appid + tid from token) |
| POST | `/api/featureflag` | Create flag |
| PUT | `/api/featureflag/{id}` | Update / toggle flag |
| DELETE | `/api/featureflag/{id}` | Delete flag |

---

## Business Logic

### CompanyBusinessLogic
- `ProvisionCompanyAsync(tenantId, companyName)` — creates company on first login, updates name if previously saved as "My Organization"
- `GetAllCompaniesAsync(ownerId)` — get companies scoped by owner
- `GetCompanyByIdAsync(id)` — get single company
- `CreateCompanyAsync(company, ownerId)` — validate name, assign ID and owner
- `UpdateCompanyAsync(id, company)` — update if exists
- `DeleteCompanyAsync(id)` — blocked if company has services

### ServiceBusinessLogic
- `CreateServiceAsync(service)` — verify company exists, use provided ID or auto-generate GUID
- `GetServicesByCompanyIdAsync(companyId)` — get services scoped by company
- `UpdateServiceAsync(id, service)` — update if exists, preserve companyId
- `DeleteServiceAsync(id)` — blocked if service has feature flags

### FeatureFlagBusinessLogic
- `CreateFeatureFlagAsync(flag)` — verify service exists, assign ID
- `GetFeatureFlagsByServiceIdAsync(serviceId)` — get flags for a service
- `UpdateFeatureFlagAsync(id, flag)` — toggle value, return updated flag
- `DeleteFeatureFlagAsync(id, serviceId)` — delete flag

---

## Cosmos DB Containers

| Container | Partition Key | Notes |
|-----------|--------------|-------|
| Companies | `/id` | ID = Azure AD tenant ID |
| Services | `/companyId` | All services for a company in same partition |
| FeatureFlags | `/serviceId` | All flags for a service in same partition |

---

## Authorization

Two authorization modes supported via `ReadFeatureFlags` policy:

| Token Type | Who uses it | Claims used |
|------------|-------------|-------------|
| Delegated user token | React UI (human login) | `scope` claim |
| App token (client credentials) | DemoService, client apps | `roles: FeatureFlags.Read` |

Both token types accepted on the same endpoints. The `my-flags` endpoint additionally extracts `appid` and `tid` to identify the calling service automatically.

---

## Error Handling

| Scenario | Behaviour |
|----------|-----------|
| Delete company with services | 400 Bad Request — "Cannot delete company because it has associated services" |
| Delete service with flags | 400 Bad Request — "Cannot delete service because it has associated feature flags" |
| Create service for non-existent company | 400 Bad Request — "Company does not exist" |
| Create flag for non-existent service | 400 Bad Request — "Service does not exist" |
| Token missing appid or tid claims | 400 Bad Request — "Token must contain appid and tid claims" |
| Cross-tenant token validation | ValidateIssuer = false — accepts tokens from any Azure AD tenant |
| Unauthorized request | 401 Unauthorized |

---

## Functional Requirements

| ID | Requirement |
|----|-------------|
| FR1 | User must sign in with Microsoft (Azure AD) to access the system |
| FR2 | Company is auto-provisioned on first login using the Azure AD tenant ID |
| FR3 | Each tenant sees only their own companies, services, and flags |
| FR4 | User can create, update, and delete companies |
| FR5 | Deleting a company is blocked if it has associated services |
| FR6 | User can create services with an optional custom Service ID |
| FR7 | Deleting a service is blocked if it has associated feature flags |
| FR8 | User can create, toggle, and delete feature flags |
| FR9 | Flag changes take effect within 10 seconds in client applications |
| FR10 | Client applications authenticate using OAuth2 client credentials flow |
| FR11 | API identifies calling service using appid and tid claims — no hardcoded IDs |
| FR12 | DemoService polls /api/featureflag/my-flags every 10 seconds and updates UI |

---

## Non-Functional Requirements

| ID | Requirement |
|----|-------------|
| NFR1 | All API endpoints require a valid Azure AD JWT token |
| NFR2 | No credentials stored — Managed Identity used for Cosmos DB access |
| NFR3 | All data transmission over HTTPS |
| NFR4 | Multi-tenant data isolation enforced at the data layer via tid claim |
| NFR5 | Cross-tenant tokens accepted (ValidateIssuer = false) |
| NFR6 | Business logic separated from controllers — testable in isolation |
| NFR7 | Repository pattern used for all Cosmos DB access |
| NFR8 | 27 unit tests covering all business logic (xUnit + Moq) |
| NFR9 | Application Insights wired up — key business events logged |
| NFR10 | GitHub Actions CI/CD — deploys API and DemoService on push to main |

---

## Out of Scope

- Percentage-based rollouts (e.g. enable for 10% of users)
- Non-boolean flag types (string, number)
- Webhook notifications on flag change
- Audit log / flag change history
- SDK for client applications
- Rate limiting on flag read endpoints

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend API | ASP.NET Core 8 Web API |
| Database | Azure Cosmos DB (NoSQL) |
| Authentication | Azure Entra ID — JWT bearer, OAuth2 client credentials |
| Frontend | React + TypeScript (Vite), MSAL |
| Demo Service | ASP.NET Core 8 minimal API |
| Hosting | Azure App Service (API + DemoService), Azure Static Web Apps (UI) |
| CI/CD | GitHub Actions |
| Monitoring | Application Insights |
| Tests | xUnit + Moq (27 unit tests) |

---

## Project Structure

```
SmartToggle/
├── SmartToggle/              # ASP.NET Core 8 Web API
│   ├── Controllers/          # CompanyController, ServiceController, FeatureFlagController
│   ├── BusinessLogic/        # Business logic + repository interfaces + implementations
│   └── Models/               # Company, Service, FeatureFlag
├── SmartToggle.DemoService/  # Cross-tenant demo app (ASP.NET Core minimal API)
├── SmartToggle.UI/           # React + TypeScript frontend
│   └── src/
│       └── pages/            # CompaniesPage, ServicesPage, FeatureFlagsPage, DemoServicePage
└── SmartToggle.Tests/        # xUnit unit tests (27 tests)
```

---

## Live URLs

| Service | URL |
|---------|-----|
| SmartToggle UI | https://white-desert-0cc31181e.7.azurestaticapps.net |
| SmartToggle API | https://smarttoggle-api.azurewebsites.net |
| Demo Service | https://smarttoggle-demoservice.azurewebsites.net |
