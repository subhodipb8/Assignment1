# Cafeteria Food Pre-ordering System

A full-stack web application for managing cafeteria food pre-orders, built with **.NET 10** (backend) and **React + TypeScript** (frontend), using **PostgreSQL** as the database.

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

## Tech Stack

### Backend
- **.NET 10** - Web API framework
- **Entity Framework Core** - ORM with PostgreSQL provider
- **PostgreSQL** - Relational database
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
├── backend/
│   └── CafeteriaAPI/
│       ├── Controllers/     # API controllers
│       ├── Data/            # DbContext
│       ├── DTOs/            # Data transfer objects
│       ├── Models/          # Entity models
│       ├── Services/        # Business logic
│       └── Program.cs       # App entry point
└── frontend/
    └── cafeteria-client/
        ├── src/
        │   ├── components/  # Reusable components
        │   ├── contexts/    # React contexts
        │   ├── pages/       # Page components
        │   ├── services/    # API services
        │   └── types/       # TypeScript types
        └── package.json
```

## Getting Started

### Prerequisites
- .NET 10 SDK
- PostgreSQL
- Node.js (v20+)
- npm

### Backend Setup

1. Navigate to backend directory:
```bash
cd backend/CafeteriaAPI
```

2. Update connection string in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=cafeteria;Username=postgres;Password=yourpassword"
}
```

3. Run migrations:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

4. Run the API:
```bash
dotnet run
```

The API will be available at `http://localhost:5000`

### Frontend Setup

1. Navigate to frontend directory:
```bash
cd frontend/cafeteria-client
```

2. Install dependencies:
```bash
npm install
```

3. Start the development server:
```bash
npm start
```

The app will be available at `http://localhost:3000`

## Demo Accounts

| Email | Password | Role |
|-------|----------|------|
| admin@cafeteria.com | admin123 | Admin |
| canteen@cafeteria.com | canteen123 | Canteen Staff |
| student@cafeteria.com | student123 | Student |

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login
- `GET /api/auth/me` - Get current user

### Menu
- `GET /api/menu` - Get all menu items
- `POST /api/menu` - Create menu item
- `PUT /api/menu/{id}` - Update menu item
- `DELETE /api/menu/{id}` - Delete menu item
- `POST /api/menu/seed` - Seed sample data

### Orders
- `GET /api/orders` - Get orders
- `POST /api/orders` - Create order
- `PUT /api/orders/{id}/status` - Update order status
- `DELETE /api/orders/{id}` - Cancel order
- `GET /api/orders/stats` - Get order statistics

### Users
- `GET /api/users/wallet` - Get wallet balance
- `POST /api/users/wallet/add` - Add funds
- `GET /api/users/preferences` - Get preferences
- `PUT /api/users/preferences` - Update preferences

## Architecture

### Database Schema

**Users Table**
- id, name, email, password_hash, role
- dietary_preferences[], allergies[]
- wallet_balance, created_at

**MenuItems Table**
- id, name, description, price, category
- dietary_tags[], allergens[]
- nutrition_info (JSON), available
- preparation_time, max_order_per_day

**Orders Table**
- id, user_id, total_amount, pickup_time
- status, payment_status, pickup_date
- special_instructions

**OrderItems Table**
- id, order_id, menu_item_id
- quantity, price

## AI Assistance Documentation

### Tools Used
- **Claude** - Primary development assistant for:
  - Code generation and architecture planning
  - Bug fixing and debugging suggestions
  - API design and implementation
  - Frontend component development

### Development Process
1. Initial architecture and database schema design
2. Backend API development with .NET and PostgreSQL
3. Frontend React component development
4. Integration and testing
5. Documentation generation

### Benefits Observed
- Faster boilerplate code generation
- Consistent code patterns and best practices
- Quick troubleshooting of errors
- Comprehensive documentation

### Challenges Encountered
- Initial Entity Framework configuration with PostgreSQL arrays
- JWT token validation setup
- CORS configuration for frontend-backend communication

## License

This project was created for educational purposes as part of the SE ZG503 Full Stack Application Development course.
