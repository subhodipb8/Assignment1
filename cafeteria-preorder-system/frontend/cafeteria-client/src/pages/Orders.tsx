import React, { useState, useEffect } from 'react';
import { orderAPI } from '../services/api';
import { Order } from '../types';
import { useAuth } from '../contexts/AuthContext';
import './Orders.css';

const Orders: React.FC = () => {
  const [orders, setOrders] = useState<Order[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [filter, setFilter] = useState('');
  const { user } = useAuth();

  useEffect(() => {
    fetchOrders();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter]);

  const fetchOrders = async () => {
    try {
      setIsLoading(true);
      setError('');
      const response = await orderAPI.getOrders(filter || undefined);
      setOrders(response.data);
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Failed to load orders';
      setError(errorMessage);
      console.error('Error loading orders:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleCancelOrder = async (orderId: number) => {
    if (!window.confirm('Are you sure you want to cancel this order?')) {
      return;
    }

    try {
      await orderAPI.cancelOrder(orderId);
      fetchOrders();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to cancel order');
    }
  };

  const handleUpdateStatus = async (orderId: number, newStatus: string) => {
    try {
      await orderAPI.updateOrderStatus(orderId, newStatus);
      fetchOrders();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to update status');
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

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  };

  if (isLoading) {
    return <div className="loading">Loading orders...</div>;
  }

  return (
    <div className="orders-container">
      <div className="orders-header">
        <h1>My Orders 📋</h1>

        <div className="filter-tabs">
          {['', 'pending', 'confirmed', 'preparing', 'ready', 'completed'].map(status => (
            <button
              key={status || 'all'}
              className={filter === status ? 'active' : ''}
              onClick={() => setFilter(status)}
            >
              {status ? status.charAt(0).toUpperCase() + status.slice(1) : 'All Orders'}
            </button>
          ))}
        </div>
      </div>

      {error && <div className="error-message">{error}</div>}

      {orders.length === 0 ? (
        <div className="no-orders">
          <span className="no-orders-icon">📭</span>
          <h2>No orders found</h2>
          <p>{filter ? `You have no ${filter} orders.` : 'Your order history is empty.'}</p>
        </div>
      ) : (
        <div className="orders-list">
          {orders.map(order => (
            <div key={order.id} className="order-card">
              <div className="order-header">
                <div className="order-info">
                  <span className="order-id">Order #{order.id}</span>
                  <span className="order-date">{formatDate(order.orderDate)}</span>
                </div>
                <span
                  className="status-badge"
                  style={{ backgroundColor: getStatusColor(order.status) }}
                >
                  {order.status.toUpperCase()}
                </span>
              </div>

              <div className="order-items">
                {order.orderItems?.map((item: any) => (
                  <div key={item.id} className="order-item">
                    <span className="item-name">{item.menuItem?.name || `Item #${item.menuItemId}`}</span>
                    <span className="item-qty">×{item.quantity}</span>
                    <span className="item-price">₹{((item.price || 0) * item.quantity).toFixed(2)}</span>
                  </div>
                ))}
              </div>

              <div className="order-footer">
                <div className="pickup-info">
                  <span>📅 Pickup: {formatDate(order.pickupDate)} at {order.pickupTime}</span>
                </div>

                <div className="order-total">
                  <span>Total: ₹{order.totalAmount.toFixed(2)}</span>
                </div>
              </div>

              {order.specialInstructions && (
                <div className="special-instructions">
                  📝 {order.specialInstructions}
                </div>
              )}

              <div className="order-actions">
                {(user?.role?.toLowerCase() === 'admin' || user?.role?.toLowerCase() === 'canteen') && order.status !== 'cancelled' && order.status !== 'completed' && (
                  <div className="status-actions">
                    {order.status === 'pending' && (
                      <button onClick={() => handleUpdateStatus(order.id, 'confirmed')}>
                        Confirm Order
                      </button>
                    )}
                    {order.status === 'confirmed' && (
                      <button onClick={() => handleUpdateStatus(order.id, 'preparing')}>
                        Start Preparing
                      </button>
                    )}
                    {order.status === 'preparing' && (
                      <button onClick={() => handleUpdateStatus(order.id, 'ready')}>
                        Mark Ready
                      </button>
                    )}
                    {order.status === 'ready' && (
                      <button onClick={() => handleUpdateStatus(order.id, 'completed')}>
                        Complete Order
                      </button>
                    )}
                  </div>
                )}

                {order.status === 'pending' && (
                  <button
                    className="cancel-btn"
                    onClick={() => handleCancelOrder(order.id)}
                  >
                    Cancel Order
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default Orders;
