# Cafeteria Pre-order System - Consolidated Swagger Documentation

## Overview

The Cafeteria Pre-order System provides both individual Swagger documentation for each microservice and a **consolidated Swagger document** that combines all API specifications into a single view.

## Quick Access

| Service | Swagger UI | JSON Spec |
|---------|------------|-----------|
| **Consolidated** | http://localhost:5000/api/swagger/ui | http://localhost:5000/api/swagger/consolidated |
| API Gateway | http://localhost:5000/swagger | http://localhost:5000/swagger/v1/swagger.json |
| Auth Service | http://localhost:5001/swagger | http://localhost:5001/swagger/v1/swagger.json |
| Menu Service | http://localhost:5002/swagger | http://localhost:5002/swagger/v1/swagger.json |
| Order Service | http://localhost:5003/swagger | http://localhost:5003/swagger/v1/swagger.json |

## Consolidated Swagger Endpoints

The API Gateway provides several endpoints for accessing consolidated API documentation:

### 1. Consolidated OpenAPI Spec

**Endpoint:** `GET /api/swagger/consolidated`

Returns a merged OpenAPI 3.0 specification containing all endpoints from:
- Auth Service (authentication, user management, wallet operations)
- Menu Service (menu items, categories, dietary information)
- Order Service (order creation, status updates, statistics)

**Features:**
- All paths are properly prefixed with service identifiers
- JWT Bearer authentication is configured
- Schemas are merged with service prefixes to avoid conflicts

### 2. Service Discovery

**Endpoint:** `GET /api/swagger/services`

Returns a list of all available services with their documentation URLs.

**Response Example:**
```json
{
  "services": [
    {
      "name": "API Gateway",
      "description": "Gateway and consolidated API documentation",
      "swaggerUrl": "http://localhost:5000/swagger/v1/swagger.json",
      "uiUrl": "http://localhost:5000/swagger"
    },
    {
      "name": "Auth Service",
      "description": "Authentication and user management",
      "swaggerUrl": "http://localhost:5001/swagger/v1/swagger.json",
      "uiUrl": "http://localhost:5001/swagger"
    }
  ]
}
```

### 3. Health Check

**Endpoint:** `GET /api/swagger/health`

Returns the health status of all microservices including response times.

**Response Example:**
```json
{
  "timestamp": "2026-05-09T10:30:00Z",
  "gateway": "healthy",
  "services": [
    {
      "service": "auth",
      "status": "healthy",
      "url": "http://localhost:5001/swagger",
      "latency_ms": 45
    }
  ]
}
```

### 4. Custom Swagger UI

**Endpoint:** `GET /api/swagger/ui`

A custom Swagger UI page that displays the consolidated API specification with quick links to individual service documentation.

## Running the Services

### Start All Services

```bash
cd /Users/subhodipanwesa/Documents/BITS_WILP/FullStack/Assignment1/cafeteria-preorder-system

# Terminal 1 - Auth Service
cd microservices/AuthService && dotnet run --urls "http://localhost:5001"

# Terminal 2 - Menu Service
cd microservices/MenuService && dotnet run --urls "http://localhost:5002"

# Terminal 3 - Order Service
cd microservices/OrderService && dotnet run --urls "http://localhost:5003"

# Terminal 4 - API Gateway
cd microservices/ApiGateway && dotnet run --urls "http://localhost:5000"
```

### Using the Makefiles

Each service has a Makefile for convenience:

```bash
cd microservices/AuthService && make run
```

## Authentication

The consolidated Swagger document supports JWT Bearer authentication:

1. Register or login via the Auth Service at `/api/auth/register` or `/api/auth/login`
2. Copy the returned JWT token
3. In Swagger UI, click the "Authorize" button
4. Enter `Bearer {your-token}` (include the word "Bearer" followed by a space)
5. Click "Authorize" to apply

## API Categories

### Auth Service APIs

| Endpoint | Description |
|----------|-------------|
| `POST /api/auth/register` | Register a new user |
| `POST /api/auth/login` | Authenticate and get JWT token |
| `GET /api/auth/me` | Get current user details |
| `GET /api/users/wallet` | Get wallet balance |
| `POST /api/users/wallet/add` | Add funds to wallet |
| `POST /api/users/wallet/deduct` | Deduct funds from wallet |
| `GET /api/users/preferences` | Get dietary preferences |
| `PUT /api/users/preferences` | Update dietary preferences |

### Menu Service APIs

| Endpoint | Description |
|----------|-------------|
| `GET /api/menu` | Get all menu items with filtering |
| `GET /api/menu/{id}` | Get menu item by ID |
| `POST /api/menu` | Create new menu item |
| `PUT /api/menu/{id}` | Update menu item |
| `DELETE /api/menu/{id}` | Delete menu item |
| `GET /api/menu/categories` | Get all unique categories |
| `POST /api/menu/seed` | Seed sample data |

### Order Service APIs

| Endpoint | Description |
|----------|-------------|
| `GET /api/orders` | Get orders (admin sees all, users see own) |
| `GET /api/orders/{id}` | Get order by ID |
| `POST /api/orders` | Create new order |
| `PUT /api/orders/{id}/status` | Update order status |
| `DELETE /api/orders/{id}` | Cancel order |
| `GET /api/orders/stats` | Get order statistics |
| `GET /api/orders/my-orders` | Get current user's orders |

## XML Documentation Features

All controllers now include comprehensive XML documentation:

- **Summary** - Brief description of the endpoint
- **Remarks** - Detailed explanation and usage notes
- **Param** - Parameter descriptions
- **Returns** - Response type description
- **Response codes** - HTTP status codes with descriptions
- **ProducesResponseType** - Typed response examples

Example:
```csharp
/// <summary>
/// Create a new menu item
/// </summary>
/// <remarks>
/// Creates a new menu item with the specified details.
/// Name is required and price must be greater than 0.
/// </remarks>
/// <param name="request">Menu item creation details</param>
/// <returns>Created menu item</returns>
/// <response code="201">Menu item created successfully</response>
/// <response code="400">Invalid input</response>
```

## Building with XML Documentation

All microservices are configured to generate XML documentation files:

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

The `NoWarn` element suppresses compiler warnings for missing XML comments on public members.

## Troubleshooting

### Service Unavailable

If a service shows as "unhealthy" in the health check:

1. Verify the service is running: `dotnet run --urls "http://localhost:PORT"`
2. Check the service logs for errors
3. Ensure all dependencies (PostgreSQL) are running

### Consolidated Spec Not Loading

If the consolidated spec fails to load:

1. Ensure all services are running
2. Check the API Gateway logs: http://localhost:5000/api/swagger/health
3. Try accessing individual service specs first
4. Verify firewall/network settings allow localhost communication

### XML Documentation Not Appearing

If Swagger UI doesn't show XML comments:

1. Build the project: `dotnet build`
2. Verify XML files are generated in `bin/Debug/net10.0/`
3. Check the file name matches: `{AssemblyName}.xml`
4. Restart the service

## Architecture

```
                    ┌─────────────────────────────────────────────┐
                    │           API Gateway (5000)                │
                    │  ┌───────────────────────────────────────┐  │
                    │  │  Swagger Aggregation Controller       │  │
                    │  │  - /api/swagger/consolidated          │  │
                    │  │  - /api/swagger/services              │  │
                    │  │  - /api/swagger/health                │  │
                    │  │  - /api/swagger/ui                    │  │
                    │  └───────────────────────────────────────┘  │
                    └──────────────────┬────────────────────────┘
                                       │
          ┌────────────────────────────┼────────────────────────────┐
          │                            │                            │
    ┌─────┴──────┐              ┌─────┴──────┐              ┌─────┴──────┐
    │ Auth       │              │ Menu       │              │ Order      │
    │ Service    │              │ Service    │              │ Service    │
    │ (5001)     │              │ (5002)     │              │ (5003)     │
    │            │              │            │              │            │
    │ /swagger   │              │ /swagger   │              │ /swagger   │
    └────────────┘              └────────────┘              └────────────┘
```

## Additional Resources

- [Unit Tests Documentation](tests/UNIT_TESTS_DOCUMENTATION.md)
- [Microservices Documentation](microservices/)
- [API Gateway Configuration](microservices/ApiGateway/ocelot.json)

---

*Last Updated: 2026-05-09*
