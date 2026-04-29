import React from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import './Home.css';

const Home: React.FC = () => {
  const { isAuthenticated, user } = useAuth();

  return (
    <div className="home-container">
      {/* Hero Section */}
      <div className="hero-section">
        <div className="floating-elements">
          <span className="floating-food">🍕</span>
          <span className="floating-food">🍔</span>
          <span className="floating-food">🥗</span>
          <span className="floating-food">🍜</span>
          <span className="floating-food">🍰</span>
        </div>

        <div className="hero-content">
          <div className="hero-badge">
            <span>🎉</span>
            <p>Now serving 10,000+ students daily!</p>
          </div>

          <h1>Delicious Food,<br />Zero Wait Time</h1>
          <p className="hero-subtitle">
            Pre-order your favorite campus meals and skip the queue.
            Fresh, fast, and exactly how you like it.
          </p>

          <div className="hero-features">
            <div className="feature-card">
              <div className="feature-icon">⚡</div>
              <h3>Quick Ordering</h3>
              <p>Order in advance and pick up at your convenience without waiting in line</p>
            </div>
            <div className="feature-card">
              <div className="feature-icon">🥗</div>
              <h3>Fresh & Healthy</h3>
              <p>Filter by vegetarian, vegan, gluten-free, and other dietary preferences</p>
            </div>
            <div className="feature-card">
              <div className="feature-icon">💳</div>
              <h3>Easy Payment</h3>
              <p>Pay seamlessly with campus wallet, cards, or digital payment methods</p>
            </div>
          </div>

          <div className="hero-actions">
            <Link to="/menu" className="btn-primary">
              Browse Menu 🍽️
            </Link>
            {!isAuthenticated && (
              <Link to="/register" className="btn-secondary">
                Get Started →
              </Link>
            )}
          </div>
        </div>
      </div>

      {/* Dashboard Section for Authenticated Users */}
      {isAuthenticated && (
        <div className="dashboard-section">
          <div className="dashboard-container">
            <div className="welcome-header">
              <h2>Welcome back, {user?.name}! 👋</h2>
              <p>Here's what you can do today</p>
            </div>
            <div className="quick-links">
              <Link to="/menu" className="quick-link-card">
                <div className="quick-link-icon">🍔</div>
                <h3>Order Food</h3>
                <p>Explore our delicious menu and place orders</p>
              </Link>
              <Link to="/orders" className="quick-link-card">
                <div className="quick-link-icon">📋</div>
                <h3>My Orders</h3>
                <p>View order history and track status</p>
              </Link>
              <Link to="/profile" className="quick-link-card">
                <div className="quick-link-icon">👤</div>
                <h3>Profile</h3>
                <p>Manage preferences and wallet</p>
              </Link>
            </div>
          </div>
        </div>
      )}

      {/* Meal Categories Section */}
      <div className="meal-categories">
        <div className="categories-container">
          <div className="section-header">
            <h2>Explore Our Menu</h2>
            <p>Delicious options for every craving and dietary preference</p>
          </div>
          <div className="categories-grid">
            <Link to="/menu" className="category-card">
              <div className="category-icon-wrapper">
                <span className="category-icon">🌅</span>
              </div>
              <h3>Breakfast</h3>
              <p>Start your day right</p>
            </Link>
            <Link to="/menu" className="category-card">
              <div className="category-icon-wrapper">
                <span className="category-icon">🍛</span>
              </div>
              <h3>Lunch</h3>
              <p>Hearty meals for energy</p>
            </Link>
            <Link to="/menu" className="category-card">
              <div className="category-icon-wrapper">
                <span className="category-icon">🍽️</span>
              </div>
              <h3>Dinner</h3>
              <p>Evening delicacies</p>
            </Link>
            <Link to="/menu" className="category-card">
              <div className="category-icon-wrapper">
                <span className="category-icon">🥪</span>
              </div>
              <h3>Snacks</h3>
              <p>Quick bites anytime</p>
            </Link>
            <Link to="/menu" className="category-card">
              <div className="category-icon-wrapper">
                <span className="category-icon">☕</span>
              </div>
              <h3>Beverages</h3>
              <p>Refresh and recharge</p>
            </Link>
          </div>
        </div>
      </div>

      {/* Stats Section */}
      <div className="stats-section">
        <div className="stats-grid">
          <div className="stat-item">
            <div className="stat-number">50+</div>
            <div className="stat-label">Menu Items</div>
          </div>
          <div className="stat-item">
            <div className="stat-number">10K+</div>
            <div className="stat-label">Daily Orders</div>
          </div>
          <div className="stat-item">
            <div className="stat-number">4.8</div>
            <div className="stat-label">User Rating</div>
          </div>
          <div className="stat-item">
            <div className="stat-number">15min</div>
            <div className="stat-label">Avg. Wait Time</div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Home;
