# Quick Setup Guide

## Prerequisites

1. **PostgreSQL** - Must be installed and running
   - Download from: https://www.postgresql.org/download/
   - Default port: 5432
   - Create a database named `cafeteria`

2. **.NET 10 SDK** - For backend
   - Download from: https://dotnet.microsoft.com/download

3. **Node.js 20+** - For frontend
   - Download from: https://nodejs.org/

## Step-by-Step Setup

### 1. PostgreSQL Database Setup

```bash
# Connect to PostgreSQL
psql -U postgres

# Create database
CREATE DATABASE cafeteria;

# Exit
\q
```

### 2. Backend Setup

```bash
# Navigate to backend
cd backend/CafeteriaAPI

# Update database connection in appsettings.json
# Edit: "DefaultConnection": "Host=localhost;Database=cafeteria;Username=postgres;Password=yourpassword"

# Restore packages (already done, but just in case)
dotnet restore

# Run the application
dotnet run

# Backend will start at: http://localhost:5000
```

### 3. Frontend Setup

Open a new terminal:

```bash
# Navigate to frontend
cd frontend/cafeteria-client

# Install dependencies (already done)
npm install

# Start the development server
npm start

# Frontend will open at: http://localhost:3000
```

## Demo Credentials

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@cafeteria.com | admin123 |
| Canteen | canteen@cafeteria.com | canteen123 |
| Student | student@cafeteria.com | student123 |

## Initial Data Setup

After logging in as **Admin** or **Canteen**:

1. Go to **Admin Panel**
2. Click on **"Seed Sample Menu Items"** button
3. This will populate the database with sample food items

## Troubleshooting

### Backend won't start
- Check if PostgreSQL is running
- Verify connection string in `appsettings.json`
- Check port 5000 is not in use

### Frontend won't connect to backend
- Ensure backend is running on port 5000
- Check CORS is enabled in backend
- Check firewall settings

### Database connection errors
- Verify PostgreSQL is accepting connections on localhost:5432
- Check username and password in connection string
- Ensure `cafeteria` database exists

## Project Structure

```
cafeteria-preorder-system/
├── backend/CafeteriaAPI/
│   ├── Controllers/      # API endpoints
│   ├── Data/             # Database context
│   ├── DTOs/             # Data transfer objects
│   ├── Models/           # Entity models
│   ├── Services/         # Business logic
│   └── Program.cs        # Entry point
│
├── frontend/cafeteria-client/
│   ├── src/
│   │   ├── components/   # React components
│   │   ├── contexts/     # Auth context
│   │   ├── pages/        # Page components
│   │   ├── services/     # API calls
│   │   └── types/        # TypeScript types
│   └── public/
│
├── README.md             # Project documentation
├── Architecture.md       # Architecture diagrams
└── AI_Usage_Log.md       # AI assistance documentation
```

## Features to Test

1. **Authentication**
   - Register new account
   - Login with credentials
   - Logout

2. **Menu Browsing**
   - View all menu items
   - Filter by category
   - Search items

3. **Ordering**
   - Add items to cart
   - Select pickup date/time
   - Checkout with wallet
   - Track order status

4. **Admin Functions**
   - View dashboard statistics
   - Seed menu items
   - Update order statuses

5. **Profile Management**
   - Add wallet funds
   - Update dietary preferences
   - Manage allergies
