# System Architecture

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        Client Layer                          │
│                   React + TypeScript                        │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐│
│  │   Pages     │  │   Services  │  │     Contexts        ││
│  │  (React)    │  │   (axios)   │  │  (AuthContext)      ││
│  └──────┬──────┘  └──────┬──────┘  └─────────────────────┘│
└─────────┼────────────────┼────────────────────────────────┘
          │                │ HTTP/REST
          │                │
          ▼                ▼
┌─────────────────────────────────────────────────────────────┐
│                      API Gateway                             │
│                   CORS Enabled                               │
├─────────────────────────────────────────────────────────────┤
│                      .NET 10 Web API                        │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐│
│  │Controllers  │  │  Services   │  │   Middleware      ││
│  │   (MVC)     │  │   (JWT)     │  │  (Auth, CORS)       ││
│  └──────┬──────┘  └─────────────┘  └─────────────────────┘│
│         │                                                    │
│         ▼                                                    │
│  ┌────────────────────────────────────────────────────────┐│
│  │           Entity Framework Core                         ││
│  │          (PostgreSQL Provider)                          ││
│  └────────────────────────┬───────────────────────────────┘│
└─────────────────────────────┼────────────────────────────────┘
                              │ SQL/TCP
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    PostgreSQL Database                       │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐│
│  │    Users    │  │  MenuItems  │  │      Orders         ││
│  │    Table    │  │    Table    │  │      Table          ││
│  └─────────────┘  └─────────────┘  └─────────────────────┘│
│                              │                             │
│  ┌───────────────────────────┴──────────────────────────┐│
│  │                   OrderItems Table                     ││
│  └────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────┘
```

## Component Hierarchy

### Frontend Component Tree

```
App
├── AuthProvider (Context)
├── Router
│   ├── Navbar
│   └── Routes
│       ├── Home
│       ├── Login
│       ├── Register
│       ├── Menu
│       ├── Cart
│       ├── Orders
│       ├── Profile
│       └── AdminPanel (Protected)
│           └── ProtectedRoute
└── Components
    ├── ProtectedRoute
    └── Navbar
```

### Backend Layer Structure

```
CafeteriaAPI
├── Controllers
│   ├── AuthController
│   ├── MenuController
│   ├── OrdersController
│   └── UsersController
├── Services
│   └── JwtService
├── Data
│   └── CafeteriaDbContext
├── Models
│   ├── User
│   ├── MenuItem
│   ├── Order
│   └── OrderItem
└── DTOs
    └── AuthDTOs
```

## Database Schema

### Entity Relationship Diagram

```
┌────────────────────┐       ┌────────────────────┐
│       users        │       │     menu_items     │
├────────────────────┤       ├────────────────────┤
│ PK  id             │       │ PK  id             │
│     name           │       │     name           │
│     email          │       │     description    │
│     password_hash  │       │     price          │
│     role           │       │     category       │
│     dietary_prefs  │       │     dietary_tags[] │
│     allergies[]    │       │     allergens[]    │
│     wallet_balance │       │     available      │
│     created_at     │       │     max_order_day  │
└────────┬───────────┘       │     orders_today   │
         │                    └────────────────────┘
         │                              │
         │  1:N                         │ 1:N
         ▼                              ▼
┌────────────────────┐       ┌────────────────────┐
│      orders        │       │    order_items     │
├────────────────────┤       ├────────────────────┤
│ PK  id             │◄──────┤ PK  id             │
│ FK  user_id        │  1:N  │ FK  order_id       │
│     total_amount   │       │ FK  menu_item_id   │
│     pickup_time    │       │     quantity       │
│     pickup_date    │       │     price          │
│     status         │       └────────────────────┘
│     payment_status │
│     special_instr  │
│     created_at     │
└────────────────────┘
```

## Data Flow

### Order Placement Flow

```
1. User Browses Menu
   Frontend → GET /api/menu → Backend → PostgreSQL

2. User Adds to Cart
   LocalStorage (State Management)

3. User Checks Out
   Frontend → POST /api/orders
              ├── Validate Wallet Balance
              ├── Validate Menu Item Availability
              ├── Create Order Record
              ├── Update MenuItem.orders_today
              ├── Deduct Wallet Balance
              └── Return Order Confirmation

4. Order Tracking
   Frontend → GET /api/orders → Real-time Status Updates
```

### Authentication Flow

```
1. Login/Register
   Frontend → POST /api/auth/login (or register)
              └── Receive JWT Token

2. Authenticated Requests
   Frontend → API Requests with Bearer Token
              └── Backend Validates JWT

3. Token Validation
   JWT Middleware → Decode Token → Extract Claims
                    └── Set User Context
```

## API Endpoints Summary

| Endpoint | Method | Description | Auth Required |
|----------|--------|-------------|---------------|
| /api/auth/register | POST | User registration | No |
| /api/auth/login | POST | User login | No |
| /api/auth/me | GET | Get current user | Yes |
| /api/menu | GET | List menu items | No |
| /api/menu | POST | Create menu item | Admin/Canteen |
| /api/menu/{id} | PUT | Update menu item | Admin/Canteen |
| /api/menu/{id} | DELETE | Delete menu item | Admin/Canteen |
| /api/orders | GET | List orders | Yes |
| /api/orders | POST | Create order | Yes |
| /api/orders/{id}/status | PUT | Update status | Admin/Canteen |
| /api/orders/{id} | DELETE | Cancel order | Yes |
| /api/users/wallet | GET | Get balance | Yes |
| /api/users/wallet/add | POST | Add funds | Yes |

## Security Considerations

1. **Authentication:** JWT tokens with 2-hour expiry
2. **Authorization:** Role-based access control (RBAC)
3. **Password Security:** BCrypt hashing
4. **CORS:** Configured for localhost:3000
5. **Input Validation:** Model validation attributes
6. **SQL Injection Prevention:** EF Core parameterized queries

## Scalability Considerations

1. **Database:** PostgreSQL supports horizontal scaling
2. **API:** Stateless design allows load balancing
3. **Frontend:** Static build for CDN deployment
4. **Caching:** Redis can be added for session storage
5. **Queue:** Message queue for order processing at scale
