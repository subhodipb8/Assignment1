import axios from 'axios';

const API_URL = 'http://localhost:5000/api';

const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add token to requests
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Auth APIs
export const authAPI = {
  register: (data: { name: string; email: string; password: string; role?: string; dietaryPreferences?: string[]; allergies?: string[] }) =>
    api.post('/auth/register', data),
  login: (data: { email: string; password: string }) =>
    api.post('/auth/login', data),
  getMe: () => api.get('/auth/me'),
};

// Menu APIs
export const menuAPI = {
  getMenuItems: (params?: { category?: string; available?: boolean; search?: string }) =>
    api.get('/menu', { params }),
  getMenuItem: (id: number) => api.get(`/menu/${id}`),
  createMenuItem: (data: any) => api.post('/menu', data),
  updateMenuItem: (id: number, data: any) => api.put(`/menu/${id}`, data),
  deleteMenuItem: (id: number) => api.delete(`/menu/${id}`),
  seedMenuItems: () => api.post('/menu/seed'),
  getCategories: () => api.get('/menu/categories'),
};

// Order APIs
export const orderAPI = {
  getOrders: (status?: string) => api.get('/orders', { params: { status } }),
  getOrder: (id: number) => api.get(`/orders/${id}`),
  createOrder: (data: any) => api.post('/orders', data),
  updateOrderStatus: (id: number, status: string) =>
    api.put(`/orders/${id}/status`, { status }),
  cancelOrder: (id: number) => api.delete(`/orders/${id}`),
  getStats: () => api.get('/orders/stats'),
};

// User APIs
export const userAPI = {
  getWalletBalance: () => api.get('/users/wallet'),
  addFunds: (amount: number) => api.post('/users/wallet/add', { amount }),
  getPreferences: () => api.get('/users/preferences'),
  updatePreferences: (data: { dietaryPreferences?: string[]; allergies?: string[] }) =>
    api.put('/users/preferences', data),
};

export default api;
