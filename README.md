# StockFlow

**StockFlow** is a web-based inventory and business management application that helps businesses organize products, stock, warehouses, customers, suppliers, and orders from one place.

🌐 **Try the live application:** [http://stockflow.runasp.net/index.html](http://stockflow.runasp.net/index.html)  
💻 **Source code:** [GitHub repository](https://github.com/Omar-adel11/StockFlow)

> Open the live link to explore the deployed application. Some features may require creating an account and signing in.

## Features

- **Dashboard** — a central view for business operations.
- **Product and category management** — organize products and their categories.
- **Inventory and warehouses** — manage stock and warehouse-related operations, including stock adjustments and transfers.
- **Customers and suppliers** — maintain business contacts and customer addresses.
- **Purchase and sales orders** — manage purchasing and sales workflows.
- **Team management** — invite and manage team members.
- **Multi-tenant access** — separate business data and access by tenant.
- **Subscription plans** — plan management, free-trial support, and subscription checkout.
- **SaaS administration** — manage business owners and platform plans.
- **Authentication and authorization** — token-based authentication and role-aware access.
- **Responsive interface** — layouts for desktop, tablet, and mobile screens.

## Technology Stack

- **Backend:** ASP.NET Core Web API (.NET 8), C#
- **Architecture:** separated application, domain, persistence, and presentation projects
- **Database:** Microsoft SQL Server with Entity Framework Core
- **Caching:** Redis
- **Frontend:** HTML, CSS, and JavaScript
- **Payments:** Paymob integration for subscription checkout

## Try StockFlow

Visit **[http://stockflow.runasp.net/index.html](http://stockflow.runasp.net/index.html)** to open the deployed application.

For authenticated features, sign in or register through the application. Access to some areas depends on your role and subscription status.

## Run Locally

### Prerequisites

- .NET 8 SDK
- Microsoft SQL Server
- Redis, if using the configured Redis-backed features
- Git

### Setup

1. Clone the repository:

   ```bash
   git clone https://github.com/Omar-adel11/StockFlow.git
   cd StockFlow
   ```

2. Open the solution in Visual Studio, or navigate to the backend project:

   ```bash
   cd Backend/StockFlow/StockFlow
   ```

3. Configure the application settings for your local environment. Set the SQL Server connection string and Redis connection string using local configuration or environment variables. Configure any required authentication, email, and payment-provider credentials through your local secrets or environment variables.

4. Apply the Entity Framework Core migrations using the migration setup for the project. Ensure the database is created and reachable before starting the API.

5. Run the application:

   ```bash
   dotnet restore
   dotnet run
   ```

   The ASP.NET Core application serves the frontend files from its `wwwroot` directory.

> **Configuration and security:** Do not commit real connection strings, JWT secrets, email credentials, or payment-provider keys. Use local user secrets or environment variables for sensitive values.

## Repository Structure

The backend is organized into separate projects:

- **Domain** — core business entities and domain logic.
- **Application** — application services, DTOs, and use-case logic.
- **Persistence** — database access and persistence concerns.
- **Presentation** — API endpoints and presentation concerns.
- **StockFlow** — ASP.NET Core host and the frontend assets in `wwwroot`.

## Contributing

1. Create a feature branch from `main`.
2. Make focused changes and test the affected workflows.
3. Open a pull request describing the changes.

## License

No license file is currently specified in this repository. Unless a license is added, reuse and redistribution are not automatically granted.
