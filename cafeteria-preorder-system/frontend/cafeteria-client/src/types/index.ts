export interface User {
  id: number;
  name: string;
  email: string;
  role: 'student' | 'staff' | 'admin' | 'canteen';
  walletBalance: number;
  dietaryPreferences: string[];
  allergies: string[];
}

export interface MenuItem {
  id: number;
  name: string;
  description: string;
  price: number;
  category: string;
  image: string;
  dietaryTags: string[];
  allergens: string[];
  nutritionInfo?: {
    calories: number;
    protein: number;
    carbs: number;
    fat: number;
  };
  available: boolean;
  preparationTime: number;
  maxOrdersPerDay: number;
  ordersToday: number;
}

export interface OrderItem {
  id: number;
  menuItemId: number;
  menuItem: MenuItem;
  quantity: number;
  price: number;
}

export interface Order {
  id: number;
  userId: number;
  user?: User;
  items?: OrderItem[];
  orderItems?: OrderItem[];
  totalAmount: number;
  pickupTime: string;
  pickupDate: string;
  status: 'pending' | 'confirmed' | 'preparing' | 'ready' | 'completed' | 'cancelled';
  paymentStatus: 'pending' | 'completed' | 'failed';
  orderDate: string;
  specialInstructions?: string;
}

export interface CartItem {
  menuItem: MenuItem;
  quantity: number;
}

export interface AuthResponse {
  token: string;
  user: User;
}
