import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import './Navbar.css';

const Navbar: React.FC = () => {
  const { user, isAuthenticated, logout } = useAuth();
  const navigate = useNavigate();
  const [menuOpen, setMenuOpen] = useState(false);

  const handleLogout = () => {
    logout();
    navigate('/');
    setMenuOpen(false);
  };

  const toggleMenu = () => {
    setMenuOpen(!menuOpen);
  };

  return (
    <nav className="navbar">
      <div className="navbar-container">
        <Link to="/" className="navbar-logo" onClick={() => setMenuOpen(false)}>
          <span className="logo-icon">🍽️</span>
          <span>Campus Cafe</span>
        </Link>

        <div className={`navbar-links ${menuOpen ? 'active' : ''}`}>
          <Link to="/menu" className="nav-link" onClick={() => setMenuOpen(false)}>
            Menu
          </Link>

          {isAuthenticated ? (
            <>
              <Link to="/cart" className="nav-link" onClick={() => setMenuOpen(false)}>
                Cart
              </Link>
              <Link to="/orders" className="nav-link" onClick={() => setMenuOpen(false)}>
                My Orders
              </Link>
              <Link to="/profile" className="nav-link" onClick={() => setMenuOpen(false)}>
                Profile
              </Link>

              {(user?.role?.toLowerCase() === 'admin' || user?.role?.toLowerCase() === 'canteen') && (
                <Link to="/admin" className="nav-link" onClick={() => setMenuOpen(false)}>
                  Admin Panel
                </Link>
              )}

              <div className="wallet-info">
                <span>💰</span>
                <span>₹{user?.walletBalance?.toFixed(2)}</span>
              </div>

              <button onClick={handleLogout} className="logout-btn">
                Logout
              </button>
            </>
          ) : (
            <>
              <Link to="/login" className="nav-link" onClick={() => setMenuOpen(false)}>
                Login
              </Link>
              <Link to="/register" className="nav-button" onClick={() => setMenuOpen(false)}>
                Sign Up
              </Link>
            </>
          )}
        </div>

        <div className="mobile-menu-toggle" onClick={toggleMenu}>
          <span style={{ transform: menuOpen ? 'rotate(45deg) translateY(5px)' : 'none' }}></span>
          <span style={{ opacity: menuOpen ? 0 : 1 }}></span>
          <span style={{ transform: menuOpen ? 'rotate(-45deg) translateY(-5px)' : 'none' }}></span>
        </div>
      </div>
    </nav>
  );
};

export default Navbar;
