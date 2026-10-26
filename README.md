# DeliveryPlatform

A multi-vendor delivery platform backend built with ASP.NET Core and .NET 10. The domain is not limited to restaurants: businesses can be restaurants, pharmacies, groceries, clothing stores, electronics shops, or another type.

The project follows Clean Architecture and uses CQRS with MediatR. It is built as a production-oriented learning project, with JWT authentication, refresh-token rotation, Google sign-in, Redis-backed token revocation, and a layered business/domain model.

## Features

### Authentication
- Email/password registration and login
- Google ID-token registration and login
- JWT access tokens (30-minute lifetime by default)
- Opaque refresh tokens with SHA-256 hashing (7-day lifetime)
- Refresh-token rotation with reuse detection
- Logout with access-token revocation via Redis
- Logout from all devices by revoking refresh tokens and incrementing the user's token version
- `TokenVersion` validation on every authenticated request

### Business management
- Create, update, and list the authenticated user's businesses
- Business types: Restaurant, Pharmacy, Grocery, Clothing, Electronics, Other
- Ownership checks on update
- Paginated "my businesses" endpoint

### Platform foundation
- Clean Architecture with API, Application, Domain, and Infrastructure layers
- CQRS handlers and FluentValidation pipeline behavior
- EF Core with SQL Server and Fluent API configurations
- Repository pattern and Unit of Work
- Centralized exception handling with consistent JSON error responses
- Swagger/OpenAPI in the Development environment
- Docker and Docker Compose support for the API, SQL Server, and Redis

## Architecture

```
DeliveryPlatform.Backend/
├── DeliveryPlatform.API/             # Controllers, middleware, DI, app configuration
├── DeliveryPlatform.Application/     # CQRS commands/queries, handlers, validators, abstractions
├── DeliveryPlatform.Domain/          # Entities, enums, domain options
├── DeliveryPlatform.Infrastructure/  # EF Core, repositories, security services, migrations
├── DeliveryPlatform.Test/            # xUnit + Moq tests
├── compose.yaml                      # API + SQL Server + Redis
├── Dockerfile
└── DeliveryPlatform.Backend.slnx
```

Dependency direction:

```
API ──────────► Application ──► Domain
 │                  ▲              ▲
 └──────► Infrastructure ──────────┘
```

- **Domain** has no project dependencies.
- **Application** depends on Domain and defines repository/security abstractions.
- **Infrastructure** implements those abstractions using EF Core, Redis, JWT, and Google APIs.
- **API** wires everything together and exposes HTTP endpoints.

## Tech Stack

| Area | Technology |
| --- | --- |
| Runtime | .NET 10 |
| Web | ASP.NET Core Web API |
| ORM | EF Core 10 (SQL Server) |
| Database | SQL Server |
| Cache | Redis |
| Messaging | MediatR 14 |
| Validation | FluentValidation 12 |
| Authentication | JWT Bearer, Google.Apis.Auth |
| Testing | xUnit, Moq |
| API docs | Swagger / OpenAPI |
| Containers | Docker, Docker Compose |

## Domain Model

| Entity | Purpose |
| --- | --- |
| `User` | Account with role (`Customer`, `BusinessOwner`, `Driver`, `Admin`) and `TokenVersion` |
| `Business` | Owned by a user; has a type, active flag, branches, categories, and products |
| `Branch` | Physical location with address, coordinates, and active flag |
| `Category` | Product category scoped to a business |
| `Product` | Business product with price, availability, and category |
| `Inventory` | Quantity and reserved quantity for a product at a branch |
| `ExternalLogins` | Provider link for external authentication (Google) |
| `UserRefreshToken` | Hashed refresh token, expiry, revocation, and replacement chain |

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, SQL Server Express, or the Docker Compose service)
- Redis 7+
- EF Core CLI tools (for migrations):

```bash
dotnet tool install --global dotnet-ef
```

Docker is optional but provides the easiest way to run SQL Server and Redis.

### Configuration

Configuration lives in `DeliveryPlatform.API/appsettings.json` and can be overridden by user secrets or environment variables.

Required sections:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=DeliveryPlatformDb;Trusted_Connection=True;TrustServerCertificate=True",
    "RedisConnection": "localhost:6379"
  },
  "JwtOptions": {
    "Issuer": "DeliveryPlatform",
    "Audience": "DeliveryPlatform.Client",
    "SecretKey": "<at-least-32-bytes>",
    "ExpirationInMinutes": 30
  },
  "GoogleAuth": {
    "ClientId": "<google-oauth-client-id>"
  }
}
```

> **Security:** do not commit real credentials. Use user secrets for local development and environment variables in deployment.

Set local secrets from the backend directory:

```bash
dotnet user-secrets --project DeliveryPlatform.API set "ConnectionStrings:DefaultConnection" "Server=.\\SQLEXPRESS;Database=DeliveryPlatformDb;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets --project DeliveryPlatform.API set "ConnectionStrings:RedisConnection" "localhost:6379"
dotnet user-secrets --project DeliveryPlatform.API set "JwtOptions:SecretKey" "<at-least-32-byte-secret>"
dotnet user-secrets --project DeliveryPlatform.API set "GoogleAuth:ClientId" "<google-oauth-client-id>"
```

The JWT secret is validated at startup: it must be non-empty and at least 32 bytes.

### Database

Apply the included EF Core migrations:

```bash
dotnet ef database update \
  --project DeliveryPlatform.Infrastructure \
  --startup-project DeliveryPlatform.API
```

Migrations live in `DeliveryPlatform.Infrastructure/Migrations`.

### Run Locally

```bash
dotnet restore DeliveryPlatform.Backend.slnx
dotnet run --project DeliveryPlatform.API --launch-profile https
```

Default URLs from `launchSettings.json`:

- HTTPS: `https://localhost:7014`
- HTTP: `http://localhost:5140`
- Swagger UI: `https://localhost:7014/swagger`

### Run with Docker Compose

Create a `.env` file in `DeliveryPlatform.Backend`:

```env
SQL_PASSWORD=<strong-sql-server-password>
JWT_SECRET=<at-least-32-byte-secret>
```

Start the stack:

```bash
docker compose up --build
```

Services:

| Service | Address |
| --- | --- |
| API | `http://localhost:5000` |
| SQL Server | `localhost,1434` |
| Redis | `localhost:6379` |

The API container uses `sqlserver` and `redis` as hostnames. Database migrations are not applied automatically, so run `dotnet ef database update` against the container database before using the API.

## API Endpoints

Base route: `/api`

### Authentication

| Method | Route | Auth | Description |
| --- | --- | --- | --- |
| `POST` | `/api/Auth/register` | No | Register with name, email, and password |
| `POST` | `/api/Auth/login` | No | Login and receive access + refresh tokens |
| `POST` | `/api/Auth/google/register` | No | Register with a Google ID token |
| `POST` | `/api/Auth/google/login` | No | Login with a Google ID token |
| `POST` | `/api/Auth/refresh` | No | Rotate a refresh token and issue new tokens |
| `POST` | `/api/Auth/logout` | Yes | Revoke the access token and a refresh token |
| `POST` | `/api/Auth/logout-all` | Yes | Revoke all sessions for the current user |

### Businesses

| Method | Route | Auth | Description |
| --- | --- | --- | --- |
| `POST` | `/api/Business` | Yes | Create a business |
| `PUT` | `/api/Business/{businessId}` | Yes | Update an owned business |
| `GET` | `/api/Business/my?pageNumber=1&pageSize=10` | Yes | List the current user's businesses |

## Request Examples

Register:

```http
POST /api/Auth/register
Content-Type: application/json

{
  "name": "Sara",
  "email": "sara@example.com",
  "password": "StrongPassword123"
}
```

Response: `201 Created`

```json
{ "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6" }
```

Login:

```http
POST /api/Auth/login
Content-Type: application/json

{
  "email": "sara@example.com",
  "password": "StrongPassword123"
}
```

Response: `200 OK`

```json
{
  "tokens": {
    "accessToken": "<jwt>",
    "refreshToken": "<opaque-token>"
  }
}
```

Create a business:

```http
POST /api/Business
Authorization: Bearer <accessToken>
Content-Type: application/json

{
  "name": "Sara's Market",
  "description": "Neighborhood grocery store",
  "businessType": 3
}
```

`businessType` uses the numeric enum values: `1` Restaurant, `2` Pharmacy, `3` Grocery, `4` Clothing, `5` Electronics, `6` Other.

Refresh tokens:

```http
POST /api/Auth/refresh
Content-Type: application/json

{ "refreshToken": "<opaque-token>" }
```

Response: `200 OK`

```json
{
  "accessToken": "<new-jwt>",
  "refreshToken": "<new-opaque-token>"
}
```

Logout:

```http
POST /api/Auth/logout
Authorization: Bearer <accessToken>
Content-Type: application/json

{ "refreshToken": "<opaque-token>" }
```

Response: `204 No Content`

## Authentication Flow

1. Login issues a short-lived JWT access token and an opaque random refresh token.
2. Only the SHA-256 hash of the refresh token is stored in `UserRefreshToken`.
3. Refreshing checks the token hash, expiry, and revocation state.
4. Rotation revokes the old token row, links it to the replacement token, and issues a new pair.
5. Reusing an already-rotated refresh token is rejected.
6. Logout stores the access token's `jti` in Redis until its expiry.
7. Logout-all increments `User.TokenVersion`, invalidating all existing access tokens, and revokes all active refresh tokens.
8. The JWT bearer pipeline checks Redis revocation and the user's current `TokenVersion` on every authenticated request.

## Error Handling

`ExceptionHandlingMiddlware` converts exceptions into JSON responses:

| Exception | Status | Meaning |
| --- | --- | --- |
| `ValidationException` | `400` | Request validation failed; includes per-field errors |
| `InvalidOperationException` | `409` | Business rule conflict, such as a duplicate email |
| `UnauthorizedAccessException` | `401` | Invalid credentials or tokens |
| Any other exception | `500` | Unexpected server error |

Example validation response:

```json
{
  "StatusCode": 400,
  "Message": "Validation failed.",
  "Errors": {
    "Email": ["'Email' is not a valid email address."]
  }
}
```

## Testing

Run the test suite:

```bash
dotnet test DeliveryPlatform.Backend.slnx
```

The `DeliveryPlatform.Test` project contains xUnit tests using Moq for business handlers and repository contracts.

## Project Status

Implemented:

- Authentication (local and Google)
- JWT access tokens and refresh-token rotation
- Redis token revocation and all-device logout
- Business create/update/list with pagination
- Domain model for businesses, branches, categories, products, and inventory
- EF Core migrations and repository/Unit of Work infrastructure
- Docker Compose for API, SQL Server, and Redis

Planned or in progress:

- Product, category, branch, and inventory endpoints
- Cart and order lifecycle
- Payment and delivery workflows
- Driver/captain assignment and location handling
- Integration tests and automated database migration on startup
- Frontend client

## Notes

- HTTP requests are redirected to HTTPS by the API; use the HTTPS profile locally or configure a reverse proxy in production.
- Swagger is enabled only in the Development environment.
- The default JWT access-token lifetime is 30 minutes and the refresh-token lifetime is 7 days.
- Keep `appsettings.json` free of production secrets. Prefer environment variables or a secrets manager.
