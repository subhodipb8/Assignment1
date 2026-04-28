import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import './Navbar.css';

const Navbar: React.FC = () => {
  const { user, isAuthenticated, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  return (
    <nav className="navbar">
      <div className="navbar-container">
        <Link to="/" className="navbar-logo">
          🍽️ Campus Cafe
        </Link>

        <div className="navbar-links">
          <Link to="/menu" className="nav-link">Menu</Link>

          {isAuthenticated ? (
            <>
              <Link to="/cart" className="nav-link">Cart</Link>
              <Link to="/orders" className="nav-link">My Orders</Link>
              <Link to="/profile" className="nav-link">Profile</Link>

              {(user?.role?.toLowerCase() === 'admin' || user?.role?.toLowerCase() === 'canteen') && (
                <Link to="/admin" className="nav-link">Admin Panel</Link>
              )}

              <div className="wallet-info">
                <span>💰 ₹{user?.walletBalance?.toFixed(2)}</span>
              </div>

              <button onClick={handleLogout} className="nav-button">
                Logout
              </button>
            </>
          ) : (
            <>
              <Link to="/login" className="nav-link">Login</Link>
              <Link to="/register" className="nav-button">Sign Up</Link>
            </>
          )}
        </div>
      </div>
    </nav>
  );
};

export default Navbar;
