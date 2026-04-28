import React, { useState, useEffect } from 'react';
import { menuAPI, orderAPI } from '../services/api';
import { useAuth } from '../contexts/AuthContext';
import { MenuItem, Order } from '../types';
import './AdminPanel.css';

const AdminPanel: React.FC = () => {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState('dashboard');
  const [stats, setStats] = useState({
    totalOrders: 0,
    todayOrders: 0,
    pendingOrders: 0,
    revenue: 0,
    todayRevenue: 0
  });
  const [orders, setOrders] = useState<Order[]>([]);
  const [menuItems, setMenuItems] = useState<MenuItem[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [message, setMessage] = useState('');

  useEffect(() => {
    fetchStats();
    fetchOrders();
    fetchMenuItems();
  }, []);

  const fetchStats = async () => {
    try {
      const response = await orderAPI.getStats();
      setStats(response.data);
    } catch (err) {
      console.error('Failed to fetch stats');
    }
  };

  const fetchOrders = async () => {
    try {
      const response = await orderAPI.getOrders();
      setOrders(response.data);
    } catch (err) {
      console.error('Failed to fetch orders');
    }
  };

  const fetchMenuItems = async () => {
    try {
      const response = await menuAPI.getMenuItems();
      setMenuItems(response.data);
    } catch (err) {
      console.error('Failed to fetch menu items');
    }
  };

  const handleSeedMenu = async () => {
    setIsLoading(true);
    try {
      const response = await menuAPI.seedMenuItems();
      setMessage(response.data.message);
      fetchMenuItems();
    } catch (err: any) {
      setMessage(err.response?.data?.message || 'Failed to seed menu');
    } finally {
      setIsLoading(false);
    }
  };

  const handleUpdateOrderStatus = async (orderId: number, status: string) => {
    try {
      await orderAPI.updateOrderStatus(orderId, status);
      setMessage(`Order #${orderId} updated to ${status}`);
      fetchOrders();
      fetchStats();
    } catch (err) {
      setMessage('Failed to update order status');
    }
  };

  const getStatusColor = (status: string) => {
    const colors: Record<string, string> = {
      'pending': '#f39c12',
      'confirmed': '#3498db',
      'preparing': '#9b59b6',
      'ready': '#2ecc71',
      'completed': '#27ae60',
      'cancelled': '#e74c3c'
    };
    return colors[status] || '#95a5a6';
  };

  return (
    <div className="admin-container">
      <div className="admin-header">
        <h1>Admin Panel ⚙️</h1>
        <p>Welcome, {user?.name} ({user?.role?.toLowerCase()})</p>
      </div>

      <div className="admin-nav">
        <button
          className={activeTab === 'dashboard' ? 'active' : ''}
          onClick={() => setActiveTab('dashboard')}
        >
          Dashboard
        </button>
        <button
          className={activeTab === 'orders' ? 'active' : ''}
          onClick={() => setActiveTab('orders')}
        >
          Manage Orders
        </button>
        <button
          className={activeTab === 'menu' ? 'active' : ''}
          onClick={() => setActiveTab('menu')}
        >
          Manage Menu
        </button>
      </div>

      {message && (
        <div className="admin-message" onClick={() => setMessage('')}>
          {message}
          <span className="close-btn">×</span>
        </div>
      )}

      {/* Dashboard Tab */}
      {activeTab === 'dashboard' && (
        <>
          <div className="dashboard-grid">
            <div className="stat-card">
              <span className="stat-icon">📦</span>
              <div className="stat-content">
                <span className="stat-value">{stats.totalOrders}</span>
                <span className="stat-label">Total Orders</span>
              </div>
            </div>

            <div className="stat-card">
              <span className="stat-icon">📅</span>
              <div className="stat-content">
                <span className="stat-value">{stats.todayOrders}</span>
                <span className="stat-label">Today's Orders</span>
              </div>
            </div>

            <div className="stat-card">
              <span className="stat-icon">⏳</span>
              <div className="stat-content">
                <span className="stat-value">{stats.pendingOrders}</span>
                <span className="stat-label">Pending Orders</span>
              </div>
            </div>

            <div className="stat-card">
              <span className="stat-icon">💰</span>
              <div className="stat-content">
                <span className="stat-value">₹{stats.revenue.toFixed(2)}</span>
                <span className="stat-label">Total Revenue</span>
              </div>
            </div>

            <div className="stat-card">
              <span className="stat-icon">💵</span>
              <div className="stat-content">
                <span className="stat-value">₹{stats.todayRevenue.toFixed(2)}</span>
                <span className="stat-label">Today's Revenue</span>
              </div>
            </div>
          </div>

          <div className="admin-actions">
            <h3>Quick Actions</h3>
            <button
              onClick={handleSeedMenu}
              disabled={isLoading}
              className="action-btn seed-btn"
            >
              {isLoading ? 'Seeding...' : '🌱 Seed Sample Menu Items'}
            </button>
          </div>
        </>
      )}

      {/* Orders Tab */}
      {activeTab === 'orders' && (
        <div className="orders-management">
          <h2>Order Management</h2>
          {orders.length === 0 ? (
            <p className="no-data">No orders found</p>
          ) : (
            <div className="orders-table-container">
              <table className="orders-table">
                <thead>
                  <tr>
                    <th>Order ID</th>
                    <th>User</th>
                    <th>Total</th>
                    <th>Status</th>
                    <th>Pickup Time</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {orders.map(order => (
                    <tr key={order.id}>
                      <td>#{order.id}</td>
                      <td>{order.user?.name || 'Unknown'}</td>
                      <td>₹{order.totalAmount.toFixed(2)}</td>
                      <td>
                        <span
                          className="status-badge"
                          style={{ backgroundColor: getStatusColor(order.status) }}
                        >
                          {order.status}
                        </span>
                      </td>
                      <td>{order.pickupTime}</td>
                      <td>
                        <div className="status-actions">
                          {order.status === 'pending' && (
                            <button
                              className="status-btn confirm"
                              onClick={() => handleUpdateOrderStatus(order.id, 'confirmed')}
                            >
                              Confirm
                            </button>
                          )}
                          {order.status === 'confirmed' && (
                            <button
                              className="status-btn prepare"
                              onClick={() => handleUpdateOrderStatus(order.id, 'preparing')}
                            >
                              Prepare
                            </button>
                          )}
                          {order.status === 'preparing' && (
                            <button
                              className="status-btn ready"
                              onClick={() => handleUpdateOrderStatus(order.id, 'ready')}
                            >
                              Ready
                            </button>
                          )}
                          {order.status === 'ready' && (
                            <button
                              className="status-btn complete"
                              onClick={() => handleUpdateOrderStatus(order.id, 'completed')}
                            >
                              Complete
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {/* Menu Tab */}
      {activeTab === 'menu' && (
        <div className="menu-management">
          <h2>Menu Management</h2>

          <div className="menu-actions">
            <button
              onClick={handleSeedMenu}
              disabled={isLoading}
              className="action-btn seed-btn"
            >
              {isLoading ? 'Seeding...' : '🌱 Seed Sample Menu Items'}
            </button>
          </div>

          {menuItems.length > 0 && (
            <div className="menu-items-list">
              <h3>Current Menu Items ({menuItems.length})</h3>
              <div className="menu-items-grid">
                {menuItems.map(item => (
                  <div key={item.id} className="menu-item-card">
                    <h4>{item.name}</h4>
                    <p className="price">₹{item.price}</p>
                    <span className={`availability ${item.available ? 'available' : 'unavailable'}`}>
                      {item.available ? 'Available' : 'Unavailable'}
                    </span>
                  </div>
                ))}
              </div>
            </div>
          )}

          <div className="instructions">
            <h3>Instructions</h3>
            <ul>
              <li>Click "Seed Sample Menu Items" to populate the database with sample menu items</li>
              <li>Navigate to the Menu page to add, edit, or delete items</li>
              <li>Check the Orders page to manage customer orders</li>
              <li>Update order statuses from "pending" → "confirmed" → "preparing" → "ready" → "completed"</li>
            </ul>
          </div>

          <div className="quick-links">
            <h3>Quick Links</h3>
            <div className="link-grid">
              <a href="/menu" className="quick-link">
                🍽️ View Menu
              </a>
              <a href="/orders" className="quick-link">
                📋 View Orders
              </a>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default AdminPanel;
