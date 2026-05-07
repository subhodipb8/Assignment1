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
          <div className="dashboard-bg-pattern"></div>
          <div className="dashboard-container">
            <div className="welcome-card">
              <div className="welcome-accent"></div>
              <div className="welcome-content">
                <div className="welcome-avatar">
                  <span>{user?.name?.charAt(0).toUpperCase() || '👤'}</span>
                </div>
                <div className="welcome-text">
                  <h2>Welcome back, {user?.name}! 👋</h2>
                  <p>Ready to {user?.role === 'admin' || user?.role === 'canteen' ? 'manage your cafeteria' : 'enjoy some delicious food'}?</p>
                  <div className="welcome-role-badge">
                    <span className={`role-dot role-${user?.role?.toLowerCase()}`}></span>
                    {user?.role?.charAt(0).toUpperCase()}{user?.role?.slice(1)}
                  </div>
                </div>
                <div className="welcome-stats">
                  <div className="mini-stat">
                    <span className="mini-stat-icon">💰</span>
                    <div>
                      <span className="mini-stat-value">₹{user?.walletBalance?.toFixed(0) || 0}</span>
                      <span className="mini-stat-label">Wallet</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <h3 className="quick-actions-title">Quick Actions</h3>
            <div className="quick-links">
              <Link to="/menu" className="quick-link-card featured">
                <div className="quick-link-badge">Popular</div>
                <div className="quick-link-icon">🍔</div>
                <h3>Browse Menu</h3>
                <p>Explore our delicious {user?.role === 'admin' || user?.role === 'canteen' ? 'offerings and manage items' : 'selection and place orders'}</p>
                <span className="quick-link-arrow">→</span>
              </Link>
              <Link to="/orders" className="quick-link-card">
                <div className="quick-link-icon">📋</div>
                <h3>{user?.role === 'admin' || user?.role === 'canteen' ? 'All Orders' : 'My Orders'}</h3>
                <p>{user?.role === 'admin' || user?.role === 'canteen' ? 'View and manage all customer orders' : 'Track orders and view history'}</p>
                <span className="quick-link-arrow">→</span>
              </Link>
              <Link to="/profile" className="quick-link-card">
                <div className="quick-link-icon">👤</div>
                <h3>My Profile</h3>
                <p>Manage preferences, wallet and settings</p>
                <span className="quick-link-arrow">→</span>
              </Link>
              {(user?.role === 'admin' || user?.role === 'canteen') && (
                <Link to="/admin" className="quick-link-card admin-card">
                  <div className="quick-link-icon">⚙️</div>
                  <h3>Admin Panel</h3>
                  <p>Manage menu items, orders, and view statistics</p>
                  <span className="quick-link-arrow">→</span>
                </Link>
              )}
            </div>
          </div>
        </div>
      )}

      {/* Meal Categories Section */}
      <div className="meal-categories">
        <div className="categories-bg-decoration"></div>
        <div className="categories-container">
          <div className="section-header">
            <div className="section-badge">
              <span>🍽️</span> Our Menu
            </div>
            <h2>Explore Delicious Options</h2>
            <p>From hearty meals to quick bites, find exactly what you're craving. Filter by dietary preferences including vegetarian, vegan, and gluten-free options.</p>
          </div>
          <div className="categories-grid">
            <Link to="/menu" className="category-card breakfast">
              <div className="category-glow"></div>
              <div className="category-icon-wrapper">
                <span className="category-icon">🌅</span>
              </div>
              <div className="category-content">
                <h3>Breakfast</h3>
                <p>Start your day with energizing breakfast options</p>
                <span className="category-cta">View Items →</span>
              </div>
              <div className="category-decoration">☀️</div>
            </Link>
            <Link to="/menu" className="category-card lunch">
              <div className="category-glow"></div>
              <div className="category-icon-wrapper">
                <span className="category-icon">🍛</span>
              </div>
              <div className="category-content">
                <h3>Lunch</h3>
                <p>Hearty meals to fuel your afternoon</p>
                <span className="category-cta">View Items →</span>
              </div>
              <div className="category-decoration">🥗</div>
            </Link>
            <Link to="/menu" className="category-card dinner">
              <div className="category-glow"></div>
              <div className="category-icon-wrapper">
                <span className="category-icon">🍽️</span>
              </div>
              <div className="category-content">
                <h3>Dinner</h3>
                <p>Delicious evening meals to end your day</p>
                <span className="category-cta">View Items →</span>
              </div>
              <div className="category-decoration">🌙</div>
            </Link>
            <Link to="/menu" className="category-card snacks">
              <div className="category-glow"></div>
              <div className="category-icon-wrapper">
                <span className="category-icon">🥪</span>
              </div>
              <div className="category-content">
                <h3>Snacks</h3>
                <p>Quick bites for anytime cravings</p>
                <span className="category-cta">View Items →</span>
              </div>
              <div className="category-decoration">🍿</div>
            </Link>
            <Link to="/menu" className="category-card beverages">
              <div className="category-glow"></div>
              <div className="category-icon-wrapper">
                <span className="category-icon">☕</span>
              </div>
              <div className="category-content">
                <h3>Beverages</h3>
                <p>Refresh with drinks and smoothies</p>
                <span className="category-cta">View Items →</span>
              </div>
              <div className="category-decoration">🥤</div>
            </Link>
          </div>

          <div className="menu-cta-section">
            <Link to="/menu" className="menu-cta-button">
              <span>🍕</span>
              <div>
                <span className="cta-title">View Full Menu</span>
                <span className="cta-subtitle">Browse all categories and dietary options</span>
              </div>
              <span className="cta-arrow">→</span>
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
