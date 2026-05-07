# Microservices Architecture

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              Client Layer                                    │
│                        React + TypeScript                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│                        API Gateway (Port 5000)                               │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │   Ocelot Gateway - Routing, Load Balancing, Authentication           │    │
│  │   ┌──────────────┐  ┌──────────────┐  ┌──────────────┐              │    │
│  │   │  Auth Routes │  │  Menu Routes │  │ Order Routes │              │    │
│  │   └──────┬───────┘  └──────┬───────┘  └──────┬───────┘              │    │
│  └──────────┼────────────────┼────────────────┼───────────────────────┘    │
└─────────────┼────────────────┼────────────────┼──────────────────────────────┘
              │                │                │
              ▼                ▼                ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                      Microservices Layer                                     │
├─────────────────┬─────────────────┬───────────────────────────────────────┤
│  Auth Service   │  Menu Service   │  Order Service                        │
│  (Port 5001)    │  (Port 5002)    │  (Port 5003)                          │
├─────────────────┼─────────────────┼───────────────────────────────────────┤
│  • User Auth    │  • Menu CRUD    │  • Order Management                   │
│  • JWT Tokens   │  • Categories   │  • Status Workflow                    │
│  • User Profile │  • Filtering    │  • Order Items                        │
│  • Wallet       │  • Search       │  • Statistics                         │
└────────┬────────┴────────┬────────┴────────┬──────────────────────────────┘
         │                   │                   │
         │   ┌───────────────┴───────────────────┘
         │   │
         ▼   ▼
┌─────────────────────────────────────────────────────────────┐
│                    PostgreSQL Databases                      │
├─────────────────┬─────────────────┬─────────────────────────┤
│ cafeteria_auth  │ cafeteria_menu  │ cafeteria_orders        │
│                 │                 │                         │
│ • users table   │ • menu_items    │ • orders                │
│                 │                 │ • order_items           │
└─────────────────┴─────────────────┴─────────────────────────┘
```

## Service Responsibilities

### API Gateway (Port 5000)
- **Technology**: Ocelot (.NET 10)
- **Responsibilities**:
  - Request routing to appropriate microservices
  - JWT authentication validation
  - CORS handling
  - Request/response transformation
  - Claims extraction and forwarding (X-User-Id, X-User-Role headers)

### Auth Service (Port 5001)
- **Database**: cafeteria_auth
- **Responsibilities**:
  - User registration and login
  - JWT token generation
  - User profile management (dietary preferences, allergies)
  - Wallet management (balance, add funds)
- **Tables**: users

### Menu Service (Port 5002)
- **Database**: cafeteria_menu
- **Responsibilities**:
  - Menu item CRUD operations
  - Category management
  - Dietary tags and allergens
  - Availability tracking
- **Tables**: menu_items

### Order Service (Port 5003)
- **Database**: cafeteria_orders
- **Responsibilities**:
  - Order creation and management
  - Order status workflow (pending → confirmed → preparing → ready → completed)
  - Order statistics
  - Order history
- **Tables**: orders, order_items

## Communication Patterns

### Synchronous (REST API)
- Frontend → API Gateway → Microservices
- Inter-service communication via HTTP (if needed)

### Data Isolation
- Each service owns its database
- No direct database access between services
- Service-to-service communication via APIs

## API Gateway Routes

| Route | Service | Port | Auth Required |
|-------|---------|------|---------------|
| /api/auth/* | Auth Service | 5001 | No* |
| /api/users/* | Auth Service | 5001 | Yes |
| /api/menu/* | Menu Service | 5002 | No (GET), Yes (POST/PUT/DELETE) |
| /api/orders/* | Order Service | 5003 | Yes |

*Except /api/auth/me which requires auth

## Authentication Flow

```
1. Login/Register
   Frontend → POST /api/auth/login (via Gateway)
                     ↓
              Auth Service (5001)
                     ↓
              JWT Token + User Info
                     ↓
              Frontend stores token

2. Authenticated Requests
   Frontend → API Request with Bearer Token
                     ↓
              API Gateway validates JWT
                     ↓
              Gateway extracts claims (userId, role)
                     ↓
              Gateway adds X-User-Id, X-User-Role headers
                     ↓
              Routes to appropriate service
                     ↓
              Service uses headers for authorization
```

## Database Schema

### Auth Service (cafeteria_auth)
```sql
users:
  - id (PK)
  - name
  - email (unique)
  - password_hash
  - role (student/staff/admin/canteen)
  - dietary_preferences[]
  - allergies[]
  - wallet_balance
  - created_at
```

### Menu Service (cafeteria_menu)
```sql
menu_items:
  - id (PK)
  - name
  - description
  - price
  - category
  - dietary_tags[]
  - allergens[]
  - available
  - preparation_time
  - max_orders_per_day
  - orders_today
  - created_at
```

### Order Service (cafeteria_orders)
```sql
orders:
  - id (PK)
  - user_id (FK to Auth Service - logical)
  - total_amount
  - pickup_time
  - pickup_date
  - status (pending/confirmed/preparing/ready/completed/cancelled)
  - payment_status (unpaid/paid/refunded)
  - special_instructions
  - created_at
  - updated_at

order_items:
  - id (PK)
  - order_id (FK)
  - menu_item_id (FK to Menu Service - logical)
  - quantity
  - price
  - menu_item_name
```

## Deployment Architecture

### Development Setup
```
┌─────────────────────────────────────────────────────────┐
│                    Development Machine                   │
├─────────────────────────────────────────────────────────┤
│  React Frontend (localhost:3000)                        │
├─────────────────────────────────────────────────────────┤
│  API Gateway (localhost:5000)                           │
├─────────────────────────────────────────────────────────┤
│  Auth Service (localhost:5001)                           │
│  Menu Service (localhost:5002)                           │
│  Order Service (localhost:5003)                          │
├─────────────────────────────────────────────────────────┤
│  PostgreSQL (localhost:5432)                            │
│  ├── cafeteria_auth                                     │
│  ├── cafeteria_menu                                     │
│  └── cafeteria_orders                                   │
└─────────────────────────────────────────────────────────┘
```

### Production Considerations
- Each service can be deployed independently
- Docker containers for each service
- Kubernetes for orchestration
- Load balancers in front of each service
- Database per service (can be on same or different PostgreSQL instances)

## Security

1. **Authentication**: JWT tokens validated at Gateway
2. **Authorization**: Role-based checks in services using X-User-Role header
3. **Data Isolation**: Service-level database isolation
4. **CORS**: Configured at Gateway level
5. **HTTPS**: Production deployments use HTTPS

## Monitoring & Observability

- Each service logs independently
- Correlation IDs passed through request chain
- Health checks for each service
- Database connection monitoring

## Benefits of Microservices Architecture

1. **Independent Scaling**: Each service can scale independently
2. **Technology Diversity**: Different services can use different tech stacks
3. **Fault Isolation**: Failure in one service doesn't affect others
4. **Team Autonomy**: Different teams can work on different services
5. **Database Per Service**: No shared database coupling
