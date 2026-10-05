# MiniBank API

Practical assignment for a Junior .NET Backend Developer position.

## Technology

- .NET 8
- ASP.NET Core Web API
- SQL Server
- Entity Framework Core
- Swagger / OpenAPI
- xUnit

## Features

- Customer management
- Multiple accounts per customer
- Deposit and withdrawal
- Money transfer between accounts
- Transaction history
- Database transaction for transfers
- DTOs for API requests/responses
- EF Core migrations
- Seed data
- Basic xUnit tests

## Requirements

Install:

1. .NET 8 SDK
2. SQL Server LocalDB (or SQL Server)
3. Visual Studio 2022 / VS Code

## Run the API

Open a terminal in the repository root:

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/MiniBank.Api
```

Swagger will be available at:

```text
https://localhost:7048/swagger
```

If the port is different on your machine, use the URL shown by `dotnet run`.

## Database

The default connection string uses SQL Server LocalDB:

```text
Server=(localdb)\MSSQLLocalDB;Database=MiniBankDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

On startup, the API automatically applies the existing EF Core migrations and inserts demo data when the database is empty.

For another SQL Server instance, edit:

`src/MiniBank.Api/appsettings.json`

## EF Core migrations

The project contains an initial migration.

To create a new migration after changing the models:

```bash
dotnet ef migrations add YourMigration --project src/MiniBank.Api --startup-project src/MiniBank.Api
```

To apply migrations manually:

```bash
dotnet ef database update --project src/MiniBank.Api --startup-project src/MiniBank.Api
```

## Demo data

On first run:

- Customer: Demo Customer
- Email: demo@minibank.local
- Account number: 10000001
- Initial balance: 1000.00

## API endpoints

### Customers

`POST /api/customers`

```json
{
  "firstName": "Ardit",
  "lastName": "Test",
  "email": "ardit@example.com"
}
```

`GET /api/customers`

`GET /api/customers/{id}`

### Accounts

`POST /api/accounts`

```json
{
  "customerId": 1,
  "accountNumber": "10000002"
}
```

`GET /api/accounts/{id}`

### Deposit

`POST /api/accounts/{id}/deposit`

```json
{
  "amount": 100
}
```

### Withdraw

`POST /api/accounts/{id}/withdraw`

```json
{
  "amount": 50
}
```

### Transfer

`POST /api/transfers`

```json
{
  "fromAccountId": 1,
  "toAccountId": 2,
  "amount": 25
}
```

### Transactions

`GET /api/accounts/{id}/transactions`

Transactions are returned newest first.

## Business rules

- Amount must be greater than zero.
- Withdrawal and transfer fail when there are insufficient funds.
- A transfer uses a database transaction: both account balances and both transaction records are committed together.
- Every deposit, withdrawal and transfer creates transaction records.
- API returns appropriate 200/201/400/404 responses.
- EF entities are not returned directly; DTOs are used.

## GitHub submission

Do not commit `bin/` or `obj/`.

The repository should contain the source code, migration, README and tests.
