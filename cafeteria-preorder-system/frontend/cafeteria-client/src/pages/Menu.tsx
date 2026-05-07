import React, { useState, useEffect } from 'react';
import { menuAPI } from '../services/api';
import { MenuItem } from '../types';
import './Menu.css';

const Menu: React.FC = () => {
  const [menuItems, setMenuItems] = useState<MenuItem[]>([]);
  const [categories, setCategories] = useState<string[]>([]);
  const [selectedCategory, setSelectedCategory] = useState('');
  const [searchQuery, setSearchQuery] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [cart, setCart] = useState<{ menuItem: MenuItem; quantity: number }[]>(
    () => JSON.parse(localStorage.getItem('cart') || '[]')
  );
  const [failedImages, setFailedImages] = useState<Set<number>>(new Set());

  useEffect(() => {
    fetchMenuItems();
    fetchCategories();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedCategory]);

  useEffect(() => {
    localStorage.setItem('cart', JSON.stringify(cart));
  }, [cart]);

  const fetchMenuItems = async () => {
    try {
      setIsLoading(true);
      setError('');
      const response = await menuAPI.getMenuItems({
        category: selectedCategory || undefined,
        available: true
      });
      setMenuItems(response.data);
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || err.message || 'Failed to load menu items';
      console.error('Menu loading error:', err);
      setError(`Failed to load menu items: ${errorMessage}`);
    } finally {
      setIsLoading(false);
    }
  };

  const fetchCategories = async () => {
    try {
      const response = await menuAPI.getCategories();
      setCategories(response.data);
    } catch (err) {
      console.error('Failed to fetch categories');
    }
  };

  const addToCart = (item: MenuItem) => {
    const existingItem = cart.find(c => c.menuItem.id === item.id);
    if (existingItem) {
      setCart(cart.map(c =>
        c.menuItem.id === item.id
          ? { ...c, quantity: c.quantity + 1 }
          : c
      ));
    } else {
      setCart([...cart, { menuItem: item, quantity: 1 }]);
    }
  };

  const filteredItems = menuItems.filter(item =>
    item.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    item.description.toLowerCase().includes(searchQuery.toLowerCase())
  );

  if (isLoading) {
    return <div className="loading">Loading menu...</div>;
  }

  return (
    <div className="menu-container">
      <div className="menu-header">
        <h1>Our Menu 🍽️</h1>

        <div className="menu-controls">
          <div className="search-box">
            <input
              type="text"
              placeholder="Search menu..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
            />
          </div>

          <div className="category-filters">
            <button
              className={selectedCategory === '' ? 'active' : ''}
              onClick={() => setSelectedCategory('')}
            >
              All
            </button>
            {categories.map(cat => (
              <button
                key={cat}
                className={selectedCategory === cat ? 'active' : ''}
                onClick={() => setSelectedCategory(cat)}
              >
                {cat.charAt(0).toUpperCase() + cat.slice(1)}
              </button>
            ))}
          </div>
        </div>
      </div>

      {cart.length > 0 && (
        <div className="cart-preview">
          <span>🛒 Cart: {cart.reduce((sum, c) => sum + c.quantity, 0)} items</span>
          <span>₹{cart.reduce((sum, c) => sum + (c.menuItem.price * c.quantity), 0).toFixed(2)}</span>
          <a href="/cart" className="checkout-btn">Checkout →</a>
        </div>
      )}

      {error && <div className="error">{error}</div>}

      <div className="menu-grid">
        {filteredItems.length === 0 ? (
          <div className="no-items">No items found matching your search.</div>
        ) : (
          filteredItems.map(item => (
            <div key={item.id} className="menu-card">
              <div className="menu-card-image">
                {item.image && !failedImages.has(item.id) ? (
                  <img
                    src={item.image}
                    alt={item.name}
                    onError={() => setFailedImages(prev => new Set(prev).add(item.id))}
                  />
                ) : (
                  <div className="placeholder-image">🍽️</div>
                )}
              </div>

              <div className="menu-card-content">
                <div className="menu-card-header">
                  <h3>{item.name}</h3>
                  <span className="price">₹{item.price}</span>
                </div>

                <p className="description">{item.description}</p>

                {item.dietaryTags?.length > 0 && (
                  <div className="dietary-tags">
                    {item.dietaryTags.map(tag => (
                      <span key={tag} className={`tag ${tag}`}>
                        {tag}
                      </span>
                    ))}
                  </div>
                )}

                {item.nutritionInfo && (
                  <div className="nutrition-info">
                    <span>{item.nutritionInfo.calories} cal</span>
                    <span>•</span>
                    <span>{item.preparationTime} min</span>
                  </div>
                )}

                <div className="menu-card-footer">
                  <span className={`availability ${item.available ? 'available' : 'unavailable'}`}>
                    {item.available ? '✓ Available' : '✗ Sold Out'}
                  </span>
                  <button
                    onClick={() => addToCart(item)}
                    disabled={!item.available}
                    className="add-btn"
                  >
                    Add to Cart
                  </button>
                </div>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
};

export default Menu;
