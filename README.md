# E-Commerce Backend API

An end-to-end backend engineering project built around an e-commerce domain.  
The goal is to evolve this repository over time into a complete backend engineering reference that covers API development, architecture, data, security, testing, and DevOps practices.

## Vision

- Build and maintain a complete backend system using production-style patterns.
- Implement backend concepts incrementally with clear, testable architecture.
- Extend the project beyond API features into deployment, automation, and operations.

## Features

- JWT authentication with access + refresh tokens
- Role-based authorization (`Admin`, `Customer`)
- Product and category management (admin-only writes, soft delete support)
- Product listing with pagination, filtering, and sorting
- Cart management for authenticated users
- Order creation from cart with stock validation
- Mock payment endpoint to simulate success/failure
- Global exception handling with ProblemDetails (`application/problem+json`)
- Swagger/OpenAPI configured with Bearer auth support
- Automatic database migration + seed data on startup

## Engineering Scope

This project is intended to cover backend engineering areas such as:

- API design and REST conventions
- Service-layer architecture and separation of concerns
- Data modeling and persistence with EF Core
- Validation, error handling, and logging
- Security with authentication/authorization
- Testing strategy (unit + integration)
- Performance and scalability improvements
- DevOps foundations (CI/CD, containerization, release workflows, observability)

## Tech Stack

- .NET 10 (`net10.0`)
- ASP.NET Core Web API
- Entity Framework Core (SQL Server)
- JWT Bearer Authentication
- BCrypt for password hashing
- Swashbuckle (Swagger UI)

## Project Structure

```text
E_Commerce_BackendAPI/
  Controllers/     # HTTP endpoints only (thin controllers)
  Services/        # Business logic + data access orchestration
  DAL/             # DbContext + seed logic
  Model/           # Entities
  Dtos/            # Request/response models
  Middleware/      # Global exception handling
  Authentication/  # Token generation service
  Utilities/       # Enums and helpers
```

## Architecture Notes

The code follows a service-layer pattern:

- Controllers handle routing, model binding, and HTTP responses.
- Services contain business logic and EF Core interaction.
- Global middleware maps exceptions to consistent error responses.

Example: order creation is handled atomically in service logic to avoid partial writes.

## Prerequisites

- .NET SDK 10.x
- SQL Server LocalDB (default connection string), or any SQL Server instance

## Configuration

Main settings file: `E_Commerce_BackendAPI/appsettings.json`

Important keys:

- `ConnectionStrings:DefaultConnection`
- `JwtSettings:SecretKey`
- `JwtSettings:Issuer`
- `JwtSettings:Audience`
- `JwtSettings:AccessTokenExpirationMinutes`

> For real environments, move secrets to environment variables or user-secrets.

## Getting Started

From the repository root:

```bash
dotnet restore
dotnet build
dotnet run --project E_Commerce_BackendAPI
```

On startup, the app:

1. Applies EF Core migrations
2. Seeds sample categories/products
3. Seeds initial data required for local development

Swagger UI is hosted at:

- [http://localhost:5000](http://localhost:5000) or [https://localhost:5001](https://localhost:5001) (depending on launch profile)

## Authentication Flow

1. Register: `POST /api/Auth/Register`
2. Login: `POST /api/Auth/Login`  
   Returns `AccessToken` + `RefreshToken`
3. Use access token in `Authorization: Bearer <token>`
4. Refresh token: `POST /api/Auth/Refresh`
5. Logout (revoke refresh token): `POST /api/Auth/Logout`

## API Overview

### Auth

- `POST /api/Auth/Register`
- `POST /api/Auth/Login`
- `POST /api/Auth/Refresh`
- `POST /api/Auth/Logout` (authorized)

### Categories

- `GET /api/Category`
- `GET /api/Category/{id}`
- `POST /api/Category` (admin)
- `PUT /api/Category/{id}` (admin)
- `DELETE /api/Category/{id}` (admin, soft delete)

### Products

- `GET /api/Product` (pagination/filter/sort)
- `GET /api/Product/{id}`
- `GET /api/Product/category/{categoryId}`
- `POST /api/Product` (admin)
- `PUT /api/Product/{id}` (admin)
- `DELETE /api/Product/{id}` (admin, soft delete)

### Cart (authorized)

- `GET /api/Cart`
- `POST /api/Cart`
- `PUT /api/Cart/{itemId}`
- `DELETE /api/Cart/{itemId}`

### Orders (authorized unless marked admin-only)

- `GET /api/Order` (my orders)
- `GET /api/Order/{id}` (my order or admin)
- `POST /api/Order` (create order from cart)
- `POST /api/Order/{id}/pay` (mock payment)
- `PUT /api/Order/{id}/status` (admin)
- `GET /api/Order/admin` (admin)

## Error Handling

Unhandled exceptions are transformed into RFC-style ProblemDetails responses by middleware:

- 400 Bad Request (`ArgumentException`)
- 401 Unauthorized (`UnauthorizedAccessException`)
- 403 Forbidden (`ForbiddenException`)
- 404 Not Found (`NotFoundException`)
- 409 Conflict (`InvalidOperationException`)
- 500 Internal Server Error (fallback)

## Roadmap

Short-term:

- Add unit tests for service-layer business logic
- Add integration tests for critical API workflows
- Introduce API versioning and caching enhancements

Mid-term:

- Add structured logging and monitoring improvements
- Add rate limiting and additional security hardening
- Improve performance with query and caching optimizations

DevOps track:

- Add CI pipeline for build/test/quality checks
- Containerize the API and standardize local environments
- Add CD workflow for staged deployments
- Introduce observability dashboards and health monitoring

## License

For educational and development purposes.
