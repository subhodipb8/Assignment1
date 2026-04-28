import React from 'react';
import './Footer.css';

const Footer: React.FC = () => {
  const currentYear = new Date().getFullYear();

  return (
    <footer className="footer">
      <div className="footer-container">
        <div className="footer-sections">
          {/* Brand Section */}
          <div className="footer-brand">
            <div className="footer-logo">
              <span className="logo-icon">🍽️</span>
              <span className="logo-text">Campus Cafe</span>
            </div>
            <p className="footer-description">
              Your campus food companion. Order delicious meals, track your orders,
              and enjoy hassle-free dining on campus.
            </p>
            <div className="footer-social">
              <span className="social-link" role="button" aria-label="Facebook">📘</span>
              <span className="social-link" role="button" aria-label="Twitter">🐦</span>
              <span className="social-link" role="button" aria-label="Instagram">📸</span>
              <span className="social-link" role="button" aria-label="LinkedIn">💼</span>
            </div>
          </div>

          {/* Quick Links */}
          <div className="footer-links">
            <h4>Quick Links</h4>
            <ul>
              <li><a href="/menu" className="footer-link">Browse Menu</a></li>
              <li><a href="/orders" className="footer-link">My Orders</a></li>
              <li><a href="/cart" className="footer-link">Cart</a></li>
              <li><a href="/profile" className="footer-link">Profile</a></li>
            </ul>
          </div>

          {/* Support */}
          <div className="footer-links">
            <h4>Support</h4>
            <ul>
              <li><span className="footer-link-text">Help Center</span></li>
              <li><span className="footer-link-text">FAQs</span></li>
              <li><span className="footer-link-text">Contact Us</span></li>
              <li><span className="footer-link-text">Feedback</span></li>
            </ul>
          </div>

          {/* Contact Info */}
          <div className="footer-contact">
            <h4>Contact Us</h4>
            <ul>
              <li>
                <span className="contact-icon">📍</span>
                <span>Campus Food Court, Building 5</span>
              </li>
              <li>
                <span className="contact-icon">📞</span>
                <span>+91 123 456 7890</span>
              </li>
              <li>
                <span className="contact-icon">✉️</span>
                <span>support@campuscafe.com</span>
              </li>
              <li>
                <span className="contact-icon">🕐</span>
                <span>Mon-Sat: 7AM - 10PM</span>
              </li>
            </ul>
          </div>
        </div>

        {/* Bottom Bar */}
        <div className="footer-bottom">
          <div className="footer-bottom-content">
            <p className="copyright">
              © {currentYear} Campus Cafe. All rights reserved.
            </p>
            <div className="footer-bottom-links">
              <span className="footer-link-text">Privacy Policy</span>
              <span className="separator">|</span>
              <span className="footer-link-text">Terms of Service</span>
              <span className="separator">|</span>
              <span className="footer-link-text">Cookie Policy</span>
            </div>
          </div>
        </div>
      </div>
    </footer>
  );
};

export default Footer;
