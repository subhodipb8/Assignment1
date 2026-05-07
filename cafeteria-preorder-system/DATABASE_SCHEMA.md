# Database Schema Documentation

## Overview
This document describes the database schema for the Cafeteria Pre-ordering System microservices architecture.

## Service: Auth Service (cafeteria_auth)

### Users Table
```sql
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    role VARCHAR(20) DEFAULT 'student',
    dietary_preferences TEXT[],
    allergies TEXT[],
    wallet_balance DECIMAL(18,2) DEFAULT 0.00,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

**Fields:**
- `id` - Primary key
- `name` - User's full name
- `email` - Unique email address
- `password_hash` - BCrypt hashed password
- `role` - User role: student, staff, admin, canteen
- `dietary_preferences` - Array of dietary tags
- `allergies` - Array of allergens
- `wallet_balance` - Digital wallet balance
- `created_at` - Account creation timestamp

---

## Service: Menu Service (cafeteria_menu)

### MenuItems Table
```sql
CREATE TABLE menu_items (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    price DECIMAL(18,2) NOT NULL,
    category VARCHAR(50),
    dietary_tags TEXT[],
    allergens TEXT[],
    available BOOLEAN DEFAULT true,
    preparation_time INTEGER DEFAULT 15,
    max_orders_per_day INTEGER DEFAULT 100,
    orders_today INTEGER DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

**Fields:**
- `id` - Primary key
- `name` - Item name
- `description` - Item description
- `price` - Price in currency
- `category` - Food category (main, beverage, etc.)
- `dietary_tags` - Array of tags (vegetarian, vegan, gluten-free)
- `allergens` - Array of allergens
- `available` - Availability status
- `preparation_time` - Time in minutes
- `max_orders_per_day` - Daily order limit
- `orders_today` - Current day's order count
- `created_at` - Creation timestamp

---

## Service: Order Service (cafeteria_orders)

### Orders Table
```sql
CREATE TABLE orders (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL,
    total_amount DECIMAL(18,2) NOT NULL,
    pickup_time TIMESTAMP NOT NULL,
    pickup_date DATE NOT NULL,
    status VARCHAR(20) DEFAULT 'pending',
    payment_status VARCHAR(20) DEFAULT 'unpaid',
    special_instructions TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP
);
```

**Fields:**
- `id` - Primary key
- `user_id` - Reference to user (from Auth Service)
- `total_amount` - Order total
- `pickup_time` - Scheduled pickup time
- `pickup_date` - Scheduled pickup date
- `status` - Order status (pending, confirmed, preparing, ready, completed, cancelled)
- `payment_status` - Payment status (unpaid, paid, refunded)
- `special_instructions` - Customer notes
- `created_at` - Order creation timestamp
- `updated_at` - Last update timestamp

### OrderItems Table
```sql
CREATE TABLE order_items (
    id SERIAL PRIMARY KEY,
    order_id INTEGER REFERENCES orders(id) ON DELETE CASCADE,
    menu_item_id INTEGER NOT NULL,
    quantity INTEGER NOT NULL,
    price DECIMAL(18,2) NOT NULL,
    menu_item_name VARCHAR(100)
);
```

**Fields:**
- `id` - Primary key
- `order_id` - Foreign key to orders
- `menu_item_id` - Reference to menu item (from Menu Service)
- `quantity` - Item quantity
- `price` - Price at time of order
- `menu_item_name` - Snapshot of item name

---

## Entity Relationship Diagram

```
┌─────────────────┐         ┌──────────────────┐
│      users      │         │    menu_items    │
├─────────────────┤         ├──────────────────┤
│ PK id           │         │ PK id            │
│    name         │         │    name          │
│    email        │         │    price         │
│    role         │         │    category      │
│    wallet       │         │    available     │
└────────┬────────┘         └──────────────────┘
         │
         │ 1:N
         ▼
┌─────────────────┐         ┌──────────────────┐
│     orders      │◄─────── │   order_items    │
├─────────────────┤    1:N  ├──────────────────┤
│ PK id           │         │ PK id            │
│ FK user_id      │         │ FK order_id      │
│    total_amount │         │    menu_item_id  │
│    status       │         │    quantity      │
│    pickup_time  │         │    price           │
└─────────────────┘         └──────────────────┘
```

## Data Flow

### User Registration/Login Flow
```
Frontend → Auth Service → cafeteria_auth DB
```

### Order Placement Flow
```
Frontend → API Gateway → Order Service → cafeteria_orders DB
                ↓
         Auth Service (validate)
                ↓
         Menu Service (verify items)
```

### Menu Management Flow
```
Admin Frontend → API Gateway → Menu Service → cafeteria_menu DB
```

## Database Per Service Pattern

Each microservice has its own isolated database:

| Service | Database | Tables |
|---------|----------|--------|
| Auth | cafeteria_auth | users |
| Menu | cafeteria_menu | menu_items |
| Order | cafeteria_orders | orders, order_items |

This ensures:
- **Data isolation** - Services cannot access each other's data directly
- **Independent scaling** - Each database can be scaled separately
- **Fault tolerance** - One database failure doesn't affect others
