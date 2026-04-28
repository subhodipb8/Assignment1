import React from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import './Home.css';

const Home: React.FC = () => {
  const { isAuthenticated, user } = useAuth();

  return (
    <div className="home-container">
      <div className="hero-section">
        <h1>Welcome to Campus Cafe 🍽️</h1>
        <p className="hero-subtitle">
          Pre-order your favorite meals and skip the queue!
        </p>

        <div className="hero-features">
          <div className="feature-card">
            <span className="feature-icon">⚡</span>
            <h3>Quick Ordering</h3>
            <p>Order in advance and pick up at your convenience</p>
          </div>
          <div className="feature-card">
            <span className="feature-icon">🥗</span>
            <h3>Dietary Preferences</h3>
            <p>Filter by vegetarian, vegan, gluten-free options</p>
          </div>
          <div className="feature-card">
            <span className="feature-icon">💳</span>
            <h3>Digital Wallet</h3>
            <p>Easy payment with campus wallet system</p>
          </div>
        </div>

        <div className="hero-actions">
          <Link to="/menu" className="btn-primary">
            Browse Menu
          </Link>
          {!isAuthenticated && (
            <Link to="/register" className="btn-secondary">
              Get Started
            </Link>
          )}
        </div>
      </div>

      {isAuthenticated && (
        <div className="dashboard-section">
          <h2>Welcome back, {user?.name}! 👋</h2>
          <div className="quick-links">
            <Link to="/menu" className="quick-link-card">
              <h3>🍔 Order Food</h3>
              <p>Explore menu and place orders</p>
            </Link>
            <Link to="/orders" className="quick-link-card">
              <h3>📋 My Orders</h3>
              <p>View order history and status</p>
            </Link>
            <Link to="/profile" className="quick-link-card">
              <h3>👤 Profile</h3>
              <p>Manage preferences and wallet</p>
            </Link>
          </div>
        </div>
      )}

      <div className="meal-categories">
        <h2>Meal Categories</h2>
        <div className="categories-grid">
          <div className="category-card">
            <span className="category-icon">🌅</span>
            <h3>Breakfast</h3>
            <p>Start your day right</p>
          </div>
          <div className="category-card">
            <span className="category-icon">🍛</span>
            <h3>Lunch</h3>
            <p>Hearty meals for energy</p>
          </div>
          <div className="category-card">
            <span className="category-icon">🍽️</span>
            <h3>Dinner</h3>
            <p>Evening delicacies</p>
          </div>
          <div className="category-card">
            <span className="category-icon">🥪</span>
            <h3>Snacks</h3>
            <p>Quick bites anytime</p>
          </div>
          <div className="category-card">
            <span className="category-icon">☕</span>
            <h3>Beverages</h3>
            <p>Refresh and recharge</p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Home;
