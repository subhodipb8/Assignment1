import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { orderAPI, userAPI } from '../services/api';
import { CartItem } from '../types';
import { useAuth } from '../contexts/AuthContext';
import './Cart.css';

const PICKUP_TIMES = [
  '08:00', '08:30', '09:00', '09:30', '10:00',
  '12:00', '12:30', '13:00', '13:30', '14:00',
  '18:00', '18:30', '19:00', '19:30', '20:00'
];

const Cart: React.FC = () => {
  const [cart, setCart] = useState<CartItem[]>(() =>
    JSON.parse(localStorage.getItem('cart') || '[]')
  );
  const [walletBalance, setWalletBalance] = useState(0);
  const [pickupTime, setPickupTime] = useState('12:00');
  const [pickupDate, setPickupDate] = useState(() => {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    return tomorrow.toISOString().split('T')[0];
  });
  const [specialInstructions, setSpecialInstructions] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const navigate = useNavigate();
  const { user } = useAuth();

  // Redirect admin/canteen users away from cart
  useEffect(() => {
    if (user?.role?.toLowerCase() === 'admin' || user?.role?.toLowerCase() === 'canteen') {
      navigate('/menu');
    }
  }, [user, navigate]);

  useEffect(() => {
    fetchWalletBalance();
  }, []);

  useEffect(() => {
    localStorage.setItem('cart', JSON.stringify(cart));
  }, [cart]);

  const fetchWalletBalance = async () => {
    try {
      const response = await userAPI.getWalletBalance();
      setWalletBalance(response.data.balance);
    } catch (err) {
      console.error('Failed to fetch wallet balance');
    }
  };

  const updateQuantity = (menuItemId: number, delta: number) => {
    setCart(cart.map(item => {
      if (item.menuItem.id === menuItemId) {
        const newQuantity = Math.max(0, item.quantity + delta);
        return { ...item, quantity: newQuantity };
      }
      return item;
    }).filter(item => item.quantity > 0));
  };

  const removeItem = (menuItemId: number) => {
    setCart(cart.filter(item => item.menuItem.id !== menuItemId));
  };

  const calculateTotal = () => {
    return cart.reduce((sum, item) => sum + (item.menuItem.price * item.quantity), 0);
  };

  const handleCheckout = async () => {
    if (cart.length === 0) {
      setError('Your cart is empty');
      return;
    }

    const total = calculateTotal();
    if (total > walletBalance) {
      setError('Insufficient wallet balance. Please add funds to your wallet.');
      return;
    }

    setIsLoading(true);
    setError('');

    try {
      const orderData = {
        items: cart.map(item => ({
          menuItemId: item.menuItem.id,
          quantity: item.quantity,
          price: item.menuItem.price,
          menuItemName: item.menuItem.name
        })),
        pickupTime: new Date(`${pickupDate}T${pickupTime}:00`).toISOString(),
        pickupDate: new Date(pickupDate).toISOString(),
        specialInstructions
      };

      await orderAPI.createOrder(orderData);
      setSuccess('Order placed successfully!');
      setCart([]);
      localStorage.removeItem('cart');

      setTimeout(() => {
        navigate('/orders');
      }, 1500);
    } catch (err: any) {
      console.error('Order error:', err);
      const errorMsg = err.response?.data?.message || err.message || 'Failed to place order. Please try again.';
      setError(`Failed to place order: ${errorMsg}`);
    } finally {
      setIsLoading(false);
    }
  };

  if (cart.length === 0 && !success) {
    return (
      <div className="cart-container">
        <div className="empty-cart">
          <span className="empty-icon">🛒</span>
          <h2>Your cart is empty</h2>
          <p>Browse our menu and add items to get started</p>
          <button onClick={() => navigate('/menu')} className="browse-btn">
            Browse Menu
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="cart-container">
      <h1>Your Cart 🛒</h1>

      {error && <div className="error-message">{error}</div>}
      {success && <div className="success-message">{success}</div>}

      <div className="cart-content">
        <div className="cart-items">
          {cart.map(item => (
            <div key={item.menuItem.id} className="cart-item">
              <div className="item-info">
                <h3>{item.menuItem.name}</h3>
                <p className="item-price">₹{item.menuItem.price}</p>
              </div>

              <div className="quantity-controls">
                <button onClick={() => updateQuantity(item.menuItem.id, -1)}>-</button>
                <span>{item.quantity}</span>
                <button onClick={() => updateQuantity(item.menuItem.id, 1)}>+</button>
              </div>

              <div className="item-total">
                ₹{(item.menuItem.price * item.quantity).toFixed(2)}
              </div>

              <button
                className="remove-btn"
                onClick={() => removeItem(item.menuItem.id)}
              >
                ×
              </button>
            </div>
          ))}
        </div>

        <div className="cart-sidebar">
          <div className="cart-summary">
            <h3>Order Summary</h3>

            <div className="summary-row">
              <span>Subtotal</span>
              <span>₹{calculateTotal().toFixed(2)}</span>
            </div>

            <div className="summary-row total">
              <span>Total</span>
              <span>₹{calculateTotal().toFixed(2)}</span>
            </div>

            <div className={`wallet-info ${calculateTotal() > walletBalance ? 'insufficient' : ''}`}>
              <span>Wallet Balance:</span>
              <span>₹{walletBalance.toFixed(2)}</span>
            </div>

            {calculateTotal() > walletBalance && (
              <div className="insufficient-funds">
                Insufficient funds. Please add ₹{(calculateTotal() - walletBalance).toFixed(2)} to your wallet.
              </div>
            )}
          </div>

          <div className="checkout-form">
            <h3>Pickup Details</h3>

            <div className="form-group">
              <label>Pickup Date</label>
              <input
                type="date"
                value={pickupDate}
                min={new Date().toISOString().split('T')[0]}
                onChange={(e) => setPickupDate(e.target.value)}
              />
            </div>

            <div className="form-group">
              <label>Pickup Time</label>
              <select value={pickupTime} onChange={(e) => setPickupTime(e.target.value)}>
                {PICKUP_TIMES.map(time => (
                  <option key={time} value={time}>{time}</option>
                ))}
              </select>
            </div>

            <div className="form-group">
              <label>Special Instructions (Optional)</label>
              <textarea
                value={specialInstructions}
                onChange={(e) => setSpecialInstructions(e.target.value)}
                placeholder="Any special requests..."
                rows={3}
              />
            </div>

            <button
              className="checkout-btn"
              onClick={handleCheckout}
              disabled={isLoading || calculateTotal() > walletBalance}
            >
              {isLoading ? 'Processing...' : `Pay ₹${calculateTotal().toFixed(2)}`}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Cart;
