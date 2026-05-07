# Component Hierarchy Documentation

## Frontend Architecture

## Component Tree

```
App
├── AuthProvider (Context)
│   └── Manages global authentication state
│
├── Router
│   ├── Navbar (Always visible when authenticated)
│   │   ├── Logo/Brand
│   │   ├── Navigation Links
│   │   ├── User Menu
│   │   └── Logout Button
│   │
│   └── Routes
│       ├── Public Routes
│       │   ├── / (Home) → Home
│       │   ├── /login → Login
│       │   └── /register → Register
│       │
│       └── Protected Routes (Require Auth)
│           ├── /menu → Menu
│           │   ├── MenuFilterBar
│           │   │   ├── SearchInput
│           │   │   └── CategoryButtons
│           │   ├── MenuGrid
│           │   │   └── MenuCard (×N)
│           │   │       ├── ItemImage
│           │   │       ├── ItemDetails
│           │   │       ├── DietaryTags
│           │   │       └── AddToCartButton
│           │   └── CartPreview
│           │
│           ├── /cart → Cart
│           │   ├── CartItemsList
│           │   │   └── CartItem (×N)
│           │   │       ├── ItemInfo
│           │   │       ├── QuantityControls
│           │   │       └── RemoveButton
│           │   ├── OrderSummary
│           │   │   ├── Subtotal
│           │   │   ├── Total
│           │   │   └── WalletBalance
│           │   ├── PickupForm
│           │   │   ├── DatePicker
│           │   │   ├── TimeSelector
│           │   │   └── SpecialInstructions
│           │   └── CheckoutButton
│           │
│           ├── /orders → Orders
│           │   ├── OrderList
│           │   │   └── OrderCard (×N)
│           │   │       ├── OrderHeader
│           │   │       ├── OrderItems
│           │   │       ├── StatusBadge
│           │   │       └── ActionButtons
│           │   └── OrderFilters
│           │
│           ├── /profile → Profile
│           │   ├── UserInfoCard
│           │   ├── WalletCard
│           │   │   ├── BalanceDisplay
│           │   │   └── AddFundsForm
│           │   └── PreferencesCard
│           │       ├── DietaryPreferences
│           │       └── AllergiesInput
│           │
│           └── /admin → AdminPanel (Admin Only)
               ├── StatsCards
               │   ├── TotalOrders
               │   ├── PendingOrders
               │   └── Revenue
               ├── MenuManagement
               │   └── MenuTable
               └── OrderManagement
                   └── OrderStatusUpdater
```

## Component Descriptions

### Layout Components

| Component | Location | Purpose |
|-----------|----------|---------|
| `Navbar` | `components/Navbar.tsx` | Top navigation bar |
| `AuthProvider` | `contexts/AuthContext.tsx` | Global auth state |
| `ProtectedRoute` | `components/ProtectedRoute.tsx` | Route guard |

### Page Components

| Component | Route | Description |
|-----------|-------|-------------|
| `Home` | `/` | Landing page with hero section |
| `Login` | `/login` | User authentication |
| `Register` | `/register` | User registration |
| `Menu` | `/menu` | Browse menu items |
| `Cart` | `/cart` | Shopping cart & checkout |
| `Orders` | `/orders` | Order history & tracking |
| `Profile` | `/profile` | User profile & preferences |
| `AdminPanel` | `/admin` | Admin dashboard |

### Shared Components

| Component | Props | Description |
|-----------|-------|-------------|
| `MenuCard` | `menuItem: MenuItem` | Display single menu item |
| `CartItem` | `item: CartItem, onUpdate, onRemove` | Cart line item |
| `OrderCard` | `order: Order` | Order summary card |
| `StatusBadge` | `status: string` | Visual status indicator |

## State Management

### Global State (AuthContext)
```typescript
interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  login: (email, password) => Promise<void>;
  register: (data) => Promise<void>;
  logout: () => void;
}
```

### Local State
- **Menu**: selectedCategory, searchQuery, cart
- **Cart**: cartItems, pickupTime, pickupDate
- **Orders**: orders, selectedFilter
- **Profile**: walletBalance, preferences

## Data Flow

### Authentication Flow
```
Login Form → authAPI.login → AuthContext → localStorage → Protected Routes
```

### Order Creation Flow
```
Menu (add to cart) → Cart (review) → orderAPI.createOrder → Orders (track)
```

### State Updates
```
API Response → Component State → UI Re-render
```

## CSS Structure

```
src/
├── components/
│   └── Navbar.css
├── pages/
│   ├── Auth.css      (Login/Register)
│   ├── Menu.css
│   ├── Cart.css
│   ├── Orders.css
│   ├── Profile.css
│   └── AdminPanel.css
└── index.css         (Global styles & variables)
```

## API Integration

### Service Layer (`services/api.ts`)
```typescript
authAPI: login, register, getMe
menuAPI: getMenuItems, getCategories, seed
orderAPI: getOrders, createOrder, updateStatus
userAPI: getWallet, addFunds, getPreferences
```

### Axios Configuration
- Base URL: `/api` (proxied to Gateway)
- Interceptors: Add auth token automatically
- Error handling: Consistent error messages

## Responsive Breakpoints

| Breakpoint | Width | Layout Changes |
|------------|-------|----------------|
| Mobile | < 640px | Single column, stacked layout |
| Tablet | 640-1024px | Two columns |
| Desktop | > 1024px | Full layout, sidebar |

## Key Design Patterns

1. **Container/Presentational Pattern**
   - Pages (containers) fetch data
   - Components (presentational) render UI

2. **Higher-Order Component**
   - `ProtectedRoute` wraps authenticated pages

3. **Custom Hooks**
   - `useAuth()` - Access auth context

4. **Context API**
   - Global auth state management
