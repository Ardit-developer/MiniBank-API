# MiniBank API 🏦

A RESTful Banking API built with **.NET 8**, **ASP.NET Core Web API**, **Entity Framework Core**, and **SQL Server**.

---

## 🏗️ Architecture & Features

This project follows clean separation of concerns and best practices:
- **Layered Architecture:** Controllers ➔ Services ➔ EF Core DbContext ➔ SQL Server
- **Data Transfer Objects (DTOs):** Strict encapsulation so EF entities are never exposed directly to consumers.
- **ACID Transactions:** Inter-account money transfers use explicit database transactions (`BeginTransactionAsync` / `CommitAsync` / `RollbackAsync`) ensuring atomicity.
- **Global Error Handling:** Centralized middleware returning clean HTTP status codes (`200`, `201`, `400`, `404`, `500`).
- **Seed Data:** Automatically pre-populates initial customers, accounts, and transactions on startup.
- **Interactive Swagger UI:** Available directly at `/` (root) upon launching.
- **Unit Tests:** Comprehensive xUnit tests covering deposits, withdrawals, and safe transfers.
- **Automated CI/CD:** GitHub Actions workflow executing build and unit tests automatically on every push.

---

## 🗄️ Database Design

- **`Customers`**: `Id`, `FirstName`, `LastName`, `Email` (unique), `CreatedAt`
- **`Accounts`**: `Id`, `AccountNumber` (unique), `CustomerId`, `Balance`, `CreatedAt`
- **`Transactions`**: `Id`, `AccountId`, `Type` (`Deposit`, `Withdraw`, `TransferIn`, `TransferOut`), `Amount`, `CreatedAt`

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB or SQL Express)

### 1. Configure Connection String
The default connection string in `src/MiniBank.Api/appsettings.json` connects to SQL Server LocalDB:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MiniBankDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

### 2. Run Database Migrations (Optional)
The application automatically creates the database and seeds initial data on startup. If you prefer to apply migrations manually via CLI:
```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project src/MiniBank.Api
dotnet ef database update --project src/MiniBank.Api
```

### 3. Run the API
```bash
dotnet run --project src/MiniBank.Api
```
Once started, open your browser and navigate to:
👉 **`http://localhost:5000`** (or the HTTPS URL printed in the console) to open **Swagger UI**.

---

## 🧪 Running Unit Tests

Run all unit tests using the .NET CLI:
```bash
dotnet test
```

---

## 📡 API Endpoints Overview

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/customers` | Create a customer |
| `GET` | `/api/customers` | List all customers |
| `GET` | `/api/customers/{id}` | Get customer details with their accounts |
| `POST` | `/api/accounts` | Open an account for a customer |
| `GET` | `/api/accounts/{id}` | Get account details & balance |
| `POST` | `/api/accounts/{id}/deposit` | Deposit money |
| `POST` | `/api/accounts/{id}/withdraw` | Withdraw money |
| `POST` | `/api/transfers` | Transfer money atomically between two accounts |
| `GET` | `/api/accounts/{id}/transactions` | List account's transactions (newest first, paginated) |
