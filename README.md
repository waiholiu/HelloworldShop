# HelloworldShop

HelloworldShop is a polished sample sandal store built with ASP.NET Core MVC and Entity Framework Core. It demonstrates a SQL-persisted catalog, anonymous cart, simulated checkout, health checks, Azure monitoring, and secretless Azure deployment.

> Checkout is a demonstration only. The application does not collect payment details or process real payments.

## Features

- Responsive product catalog and product details
- Anonymous carts stored in the database
- Simulated checkout with persisted orders and order items
- Idempotent catalog seed data and EF Core migrations
- `/health` application and database health endpoint
- Application Insights telemetry
- Azure SQL authentication through App Service managed identity
- GitHub Actions CI and OIDC-based App Service deployment

## Architecture

| Component | Technology |
|-----------|------------|
| Web application | ASP.NET Core MVC on Linux Azure App Service |
| Data | EF Core with Azure SQL Database |
| Identity | App Service system-assigned managed identity |
| Monitoring | Workspace-based Application Insights and Log Analytics |
| Secret boundary | Azure Key Vault with RBAC |
| Delivery | Azure Developer CLI, Bicep, and GitHub Actions OIDC |

Azure SQL is configured for Microsoft Entra-only authentication. No SQL administrator password, publish profile, client secret, or other long-lived deployment credential is used.

## Local development

Prerequisites:

- .NET 8 SDK

Run the application:

```powershell
dotnet restore HelloworldShop.sln
dotnet run --project src/HelloworldShop.Web
```

The local development profile uses a local database. Production receives its Azure SQL connection string from App Service configuration.

Run tests:

```powershell
dotnet test HelloworldShop.sln
```

## Azure deployment

The deployment source of truth is [`.azure/deployment-plan.md`](.azure/deployment-plan.md). Infrastructure is defined in [`infra/`](infra), with:

- Linux App Service Free F1 plan
- Azure SQL Database Basic tier
- Application Insights and Log Analytics
- Azure Key Vault with purge protection and RBAC
- Managed identity access to SQL and Key Vault

Deployments use the sequence `azure-prepare` → `azure-validate` → `azure-deploy`. The `main` branch deployment workflow uses GitHub's short-lived OIDC token and is restricted to `waiholiu/HelloworldShop`.

## Security notes

- HTTPS and TLS 1.2 or later are required.
- FTPS is disabled.
- Azure SQL accepts Microsoft Entra authentication only.
- The application identity receives only the SQL roles needed by the application.
- Key Vault uses Azure RBAC, soft delete, and purge protection.
- This public demo must not be used to store payment details or sensitive personal data.
