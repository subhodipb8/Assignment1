# Cafeteria Pre-ordering System - Microservices Architecture

A full-stack web application for managing cafeteria food pre-orders, built with a **microservices architecture** using **.NET 10** (backend services), **React + TypeScript** (frontend), and **PostgreSQL** databases.

## Features

### Students/Users
- Browse menu with category filters and search
- View dietary tags (vegetarian, vegan, gluten-free, etc.) and allergens
- Pre-order food with pickup time selection
- Digital wallet for payments
- Track order status (pending → confirmed → preparing → ready → completed)
- Dietary preferences and allergy management

### Canteen Staff/Admin
- Manage menu items (CRUD operations)
- Update order status through workflow
- View dashboard statistics
- Real-time order tracking

## Microservices Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                      Client Layer                           │
│                  React + TypeScript                          │
└───────────────────────┬────────────────────────────────────┘
                        │
                        ▼
┌──────────────────────────────────────────────────────────────┐
│                   API Gateway (Port 5000)                    │
│                   Ocelot + JWT Auth                          │
└───────────────────────┬────────────────────────────────────┘
                        │
         ┌──────────────┼──────────────┐
         │              │              │
         ▼              ▼              ▼
┌─────────────┐ ┌─────────────┐ ┌─────────────┐
│   Auth      │ │   Menu      │ │   Order     │
│  Service    │ │  Service    │ │  Service    │
│ (Port 5001) │ │ (Port 5002) │ │ (Port 5003) │
└──────┬──────┘ └──────┬──────┘ └──────┬──────┘
       │               │               │
       └───────────────┼───────────────┘
                       │
                       ▼
        ┌────────────────────────────┐
        │      PostgreSQL            │
        │  ┌─────┬─────┬─────┐       │
        │  │auth │menu │order│       │
        │  └─────┴─────┴─────┘       │
        └────────────────────────────┘
```

## Tech Stack

### Microservices (Backend)
- **.NET 10** - Web API framework
- **Ocelot** - API Gateway for routing and load balancing
- **Entity Framework Core** - ORM with PostgreSQL provider
- **PostgreSQL** - Relational databases (one per service)
- **JWT Authentication** - Token-based auth
- **BCrypt** - Password hashing

### Frontend
- **React 19** - UI library
- **TypeScript** - Type safety
- **React Router** - Client-side routing
- **Axios** - HTTP client
- **CSS Modules** - Styling

## Project Structure

```
cafeteria-preorder-system/
├── microservices/
│   ├── ApiGateway/           # API Gateway (Ocelot)
│   │   ├── Program.cs
│   │   ├── ocelot.json       # Route configuration
│   │   └── DelegatingHandlers/
│   │       └── UserContextHandler.cs
│   ├── AuthService/          # Authentication & User Management
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   └── UsersController.cs
│   │   ├── Models/
│   │   ├── DTOs/
│   │   ├── Data/
│   │   └── Services/
│   ├── MenuService/          # Menu Management
│   │   ├── Controllers/
│   │   │   └── MenuController.cs
│   │   ├── Models/
│   │   ├── DTOs/
│   │   └── Data/
│   └── OrderService/         # Order Management
│       ├── Controllers/
│       │   └── OrdersController.cs
│       ├── Models/
│       ├── DTOs/
│       └── Data/
└── frontend/
    └── cafeteria-client/     # React frontend
        ├── src/
        │   ├── components/
        │   ├── contexts/
        │   ├── pages/
        │   └── services/
        └── package.json
```

## Getting Started

### Prerequisites
- .NET 10 SDK
- PostgreSQL
- Node.js (v20+)
- npm

### Database Setup

Create three separate databases:

```sql
CREATE DATABASE cafeteria_auth;
CREATE DATABASE cafeteria_menu;
CREATE DATABASE cafeteria_orders;
```

### Backend Setup

#### 1. Auth Service (Port 5001)

```bash
cd microservices/AuthService

# Update connection string in appsettings.json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=cafeteria_auth;Username=postgres;Password=yourpassword"
}

# Run migrations
dotnet ef migrations add InitialCreate
dotnet ef database update

# Start service
dotnet run --urls "http://localhost:5001"
```

#### 2. Menu Service (Port 5002)

```bash
cd microservices/MenuService

# Update connection string
dotnet ef migrations add InitialCreate
dotnet ef database update

# Start service
dotnet run --urls "http://localhost:5002"
```

#### 3. Order Service (Port 5003)

```bash
cd microservices/OrderService

# Update connection string
dotnet ef migrations add InitialCreate
dotnet ef database update

# Start service
dotnet run --urls "http://localhost:5003"
```

#### 4. API Gateway (Port 5000)

```bash
cd microservices/ApiGateway

# Start gateway
dotnet run --urls "http://localhost:5000"
```

**Note**: Services must be started in order: Auth → Menu → Order → Gateway

### Frontend Setup

```bash
cd frontend/cafeteria-client

# Install dependencies
npm install

# Start development server
npm start
```

The app will be available at `http://localhost:3000`

## Service URLs

| Service | URL | Description |
|---------|-----|-------------|
| API Gateway | http://localhost:5010 | Entry point for all requests |
| Auth Service | http://localhost:5001 | Direct access (dev only) |
| Menu Service | http://localhost:5002 | Direct access (dev only) |
| Order Service | http://localhost:5003 | Direct access (dev only) |

## API Endpoints

All endpoints go through the API Gateway (http://localhost:5000)

### Authentication
| Endpoint | Method | Description | Auth |
|----------|--------|-------------|------|
| `/api/auth/register` | POST | Register new user | No |
| `/api/auth/login` | POST | Login | No |
| `/api/auth/me` | GET | Get current user | Yes |

### Users
| Endpoint | Method | Description | Auth |
|----------|--------|-------------|------|
| `/api/users/wallet` | GET | Get wallet balance | Yes |
| `/api/users/wallet/add` | POST | Add funds | Yes |
| `/api/users/preferences` | GET/PUT | Get/Update preferences | Yes |

### Menu
| Endpoint | Method | Description | Auth |
|----------|--------|-------------|------|
| `/api/menu` | GET | Get all menu items | No |
| `/api/menu` | POST | Create menu item | Admin only |
| `/api/menu/{id}` | GET/PUT/DELETE | CRUD operations | Varies |
| `/api/menu/categories` | GET | Get all categories | No |
| `/api/menu/seed` | POST | Seed sample data | No |

### Orders
| Endpoint | Method | Description | Auth |
|----------|--------|-------------|------|
| `/api/orders` | GET | Get all orders | Yes (Admin) |
| `/api/orders` | POST | Create order | Yes |
| `/api/orders/{id}` | GET | Get order by ID | Yes |
| `/api/orders/{id}/status` | PUT | Update status | Yes (Admin) |
| `/api/orders/{id}` | DELETE | Cancel order | Yes |
| `/api/orders/stats` | GET | Get statistics | Yes |
| `/api/orders/my-orders` | GET | Get current user's orders | Yes |

## Demo Accounts

| Email | Password | Role |
|-------|----------|------|
| admin@cafeteria.com | admin123 | admin |
| canteen@cafeteria.com | canteen123 | canteen |
| student@cafeteria.com | student123 | student |

## Architecture Benefits

1. **Independent Deployment**: Each service can be deployed separately
2. **Technology Diversity**: Each service could use different tech stacks if needed
3. **Fault Isolation**: Failure in one service doesn't cascade
4. **Scalability**: Scale services independently based on load
5. **Database Isolation**: Each service owns its data

## AI Assistance Documentation

### Tools Used
- **Claude** - Primary development assistant for:
  - Microservices architecture design
  - Code generation and refactoring
  - API Gateway configuration
  - Database schema design
  - Documentation

### Development Process
1. Converted monolithic architecture to microservices
2. Created API Gateway with Ocelot
3. Split backend into Auth, Menu, and Order services
4. Each service with its own database
5. Configured inter-service communication via HTTP
6. Updated frontend to work with Gateway

### Benefits Observed
- Faster architecture decisions
- Consistent code patterns across services
- Clear separation of concerns
- Scalable design for future growth

### Challenges Encountered
- Service-to-service communication design
- Database per service pattern implementation
- JWT validation at Gateway level
- Claims forwarding to downstream services

## License

This project was created for educational purposes as part of the SE ZG503 Full Stack Application Development course.
