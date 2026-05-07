# Microservices Setup Guide

This guide explains how to set up and run the Cafeteria Pre-ordering System with its microservices architecture.

## Architecture Overview

```
┌──────────────┐
│   Frontend   │  http://localhost:3000
│   (React)    │
└──────┬───────┘
       │
       │ All API calls
       ▼
┌──────────────┐
│ API Gateway  │  http://localhost:5000
│   (Ocelot)   │
└──────┬───────┘
       │ Routes requests
   ┌───┴───┐
   │       │
   ▼       ▼       ▼
┌─────┐ ┌─────┐ ┌─────┐
│Auth │ │Menu │ │Order│
│5001 │ │5002 │ │5003 │
└──┬──┘ └──┬──┘ └──┬──┘
   │       │       │
   ▼       ▼       ▼
┌─────┐ ┌─────┐ ┌─────┐
│auth │ │menu │ │order│
│ DB  │ │ DB  │ │ DB  │
└─────┘ └─────┘ └─────┘
```

## Prerequisites

- .NET 10 SDK
- PostgreSQL (running locally or via Docker)
- Node.js v20+ and npm
- (Optional) Docker for containerized deployment

## Database Setup

Connect to PostgreSQL and create three databases:

```sql
-- Connect as postgres user
psql -U postgres

-- Create databases
CREATE DATABASE cafeteria_auth;
CREATE DATABASE cafeteria_menu;
CREATE DATABASE cafeteria_orders;

-- Verify
\l
```

## Step-by-Step Service Setup

### Step 1: Auth Service (Port 5001)

```bash
cd microservices/AuthService

# Restore packages
dotnet restore

# Create initial migration
dotnet ef migrations add InitialCreate

# Apply migration
dotnet ef database update

# Run the service
dotnet run --urls "http://localhost:5001"
```

**Verify:** Open http://localhost:5001/api/auth (should return 404 or method not allowed)

### Step 2: Menu Service (Port 5002)

```bash
cd microservices/MenuService

# Restore packages
dotnet restore

# Create initial migration
dotnet ef migrations add InitialCreate

# Apply migration
dotnet ef database update

# Run the service
dotnet run --urls "http://localhost:5002"
```

**Verify:** Open http://localhost:5002/api/menu (should return empty array)

**Seed sample data:**
```bash
curl -X POST http://localhost:5002/api/menu/seed
```

### Step 3: Order Service (Port 5003)

```bash
cd microservices/OrderService

# Restore packages
dotnet restore

# Create initial migration
dotnet ef migrations add InitialCreate

# Apply migration
dotnet ef database update

# Run the service
dotnet run --urls "http://localhost:5003"
```

**Verify:** Open http://localhost:5003/api/orders (should return empty array)

### Step 4: API Gateway (Port 5000)

```bash
cd microservices/ApiGateway

# Restore packages
dotnet restore

# Run the gateway
dotnet run --urls "http://localhost:5000"
```

**Verify:** Open http://localhost:5000/api/menu (should proxy to Menu service)

### Step 5: Frontend

```bash
cd frontend/cafeteria-client

# Install dependencies
npm install

# Start the development server
npm start
```

**Access:** Open http://localhost:3000 in browser

## Configuration

### Auth Service (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=cafeteria_auth;Username=postgres;Password=YOUR_PASSWORD"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyHere12345678901234567890",
    "Issuer": "CafeteriaAuth",
    "Audience": "CafeteriaUsers",
    "ExpiryHours": "2"
  }
}
```

### Menu Service (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=cafeteria_menu;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

### Order Service (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=cafeteria_orders;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

## Running All Services

### Option 1: Multiple Terminals

Open 5 terminal windows/tabs:

**Terminal 1 - Auth Service:**
```bash
cd microservices/AuthService
dotnet run --urls "http://localhost:5001"
```

**Terminal 2 - Menu Service:**
```bash
cd microservices/MenuService
dotnet run --urls "http://localhost:5002"
```

**Terminal 3 - Order Service:**
```bash
cd microservices/OrderService
dotnet run --urls "http://localhost:5003"
```

**Terminal 4 - API Gateway:**
```bash
cd microservices/ApiGateway
dotnet run --urls "http://localhost:5000"
```

**Terminal 5 - Frontend:**
```bash
cd frontend/cafeteria-client
npm start
```

### Option 2: tmux Script (Linux/Mac)

Create `start-services.sh`:

```bash
#!/bin/bash

cd "$(dirname "$0")"

tmux new-session -d -s cafeteria

tmux split-window -h
tmux split-window -v
tmux select-pane -t 0
tmux split-window -v

# Pane 0: Auth Service
tmux send-keys -t 0 'cd microservices/AuthService && dotnet run --urls "http://localhost:5001"' C-m

# Pane 1: Menu Service
tmux send-keys -t 1 'cd microservices/MenuService && dotnet run --urls "http://localhost:5002"' C-m

# Pane 2: Order Service
tmux send-keys -t 2 'cd microservices/OrderService && dotnet run --urls "http://localhost:5003"' C-m

# Pane 3: API Gateway
tmux send-keys -t 3 'cd microservices/ApiGateway && dotnet run --urls "http://localhost:5000"' C-m

tmux attach-session -t cafeteria
```

### Option 3: Docker Compose (Recommended for Production)

Create `docker-compose.yml`:

```yaml
version: '3.8'

services:
  postgres-auth:
    image: postgres:15
    environment:
      POSTGRES_DB: cafeteria_auth
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: password
    ports:
      - "5433:5432"

  postgres-menu:
    image: postgres:15
    environment:
      POSTGRES_DB: cafeteria_menu
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: password
    ports:
      - "5434:5432"

  postgres-orders:
    image: postgres:15
    environment:
      POSTGRES_DB: cafeteria_orders
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: password
    ports:
      - "5435:5432"

  auth-service:
    build: ./microservices/AuthService
    ports:
      - "5001:5001"
    environment:
      - ASPNETCORE_URLS=http://+:5001
      - ConnectionStrings__DefaultConnection=Host=postgres-auth;Database=cafeteria_auth;Username=postgres;Password=password
    depends_on:
      - postgres-auth

  menu-service:
    build: ./microservices/MenuService
    ports:
      - "5002:5002"
    environment:
      - ASPNETCORE_URLS=http://+:5002
      - ConnectionStrings__DefaultConnection=Host=postgres-menu;Database=cafeteria_menu;Username=postgres;Password=password
    depends_on:
      - postgres-menu

  order-service:
    build: ./microservices/OrderService
    ports:
      - "5003:5003"
    environment:
      - ASPNETCORE_URLS=http://+:5003
      - ConnectionStrings__DefaultConnection=Host=postgres-orders;Database=cafeteria_orders;Username=postgres;Password=password
    depends_on:
      - postgres-orders

  api-gateway:
    build: ./microservices/ApiGateway
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_URLS=http://+:5000
    depends_on:
      - auth-service
      - menu-service
      - order-service

  frontend:
    build: ./frontend/cafeteria-client
    ports:
      - "3000:3000"
    depends_on:
      - api-gateway
```

Run with:
```bash
docker-compose up --build
```

## Testing the Services

### 1. Register a User

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Student",
    "email": "student@test.com",
    "password": "password123",
    "role": "student"
  }'
```

### 2. Login

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "student@test.com",
    "password": "password123"
  }'
```

Save the token from the response.

### 3. Get Menu (No Auth Required)

```bash
curl http://localhost:5000/api/menu
```

### 4. Create Order (Auth Required)

```bash
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "items": [
      {"menuItemId": 1, "quantity": 2, "price": 8.99}
    ],
    "pickupTime": "12:30",
    "pickupDate": "2026-05-08"
  }'
```

## Troubleshooting

### Port Already in Use

If a port is already in use, kill the process:

```bash
# Find process using port 5000
lsof -i :5000

# Kill the process
kill -9 <PID>
```

### Database Connection Failed

1. Verify PostgreSQL is running:
   ```bash
   pg_isready
   ```

2. Check connection string in appsettings.json

3. Verify database exists:
   ```bash
   psql -U postgres -c "\l"
   ```

### CORS Errors

Ensure API Gateway is running and CORS is configured. Frontend should call the Gateway (port 5000), not individual services.

### JWT Authentication Failed

1. Check JWT configuration is identical across all services
2. Ensure token hasn't expired (default: 2 hours)
3. Verify token is being passed in Authorization header

### Services Can't Connect to Each Other

If running in Docker, use service names as hostnames:
- `http://auth-service:5001` instead of `http://localhost:5001`
- `http://menu-service:5002` instead of `http://localhost:5002`

## Development Workflow

### Making Changes to a Service

1. Stop the service
2. Make code changes
3. Run `dotnet build` to check for errors
4. Restart the service
5. Test through the Gateway (port 5000)

### Adding a New Endpoint

1. Add to appropriate service controller
2. Update `ocelot.json` if endpoint needs Gateway routing
3. Add authentication attributes if required
4. Test via Gateway

### Database Migrations

When changing models:

```bash
cd microservices/YourService
dotnet ef migrations add MigrationName
dotnet ef database update
```

## Service Health Checks

| Service | Health URL | Expected Response |
|---------|------------|-------------------|
| Auth | http://localhost:5001/api/auth/login | 400 (validation error) |
| Menu | http://localhost:5002/api/menu | 200 + [] or menu items |
| Order | http://localhost:5003/api/orders | 200 + [] or orders |
| Gateway | http://localhost:5000/api/menu | 200 (proxies to Menu) |

## Shutdown

### Individual Services

Press `Ctrl+C` in each terminal window.

### tmux Session

```bash
tmux kill-session -t cafeteria
```

### Docker Compose

```bash
docker-compose down
```

To remove volumes (deletes all data):
```bash
docker-compose down -v
```
