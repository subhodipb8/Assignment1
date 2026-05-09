import React, { useState, useEffect } from 'react';
import { menuAPI } from '../services/api';
import { MenuItem } from '../types';
import { useAuth } from '../contexts/AuthContext';
import './Menu.css';

const Menu: React.FC = () => {
  const [menuItems, setMenuItems] = useState<MenuItem[]>([]);
  const [categories, setCategories] = useState<string[]>([]);
  const [selectedCategory, setSelectedCategory] = useState('');
  const [searchQuery, setSearchQuery] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');
  const [cart, setCart] = useState<{ menuItem: MenuItem; quantity: number }[]>(
    () => JSON.parse(localStorage.getItem('cart') || '[]')
  );
  const [failedImages, setFailedImages] = useState<Set<number>>(new Set());
  const { user } = useAuth();
  const [showAddModal, setShowAddModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [editingItem, setEditingItem] = useState<MenuItem | null>(null);
  const [showAllItems, setShowAllItems] = useState(false);

  const isAdmin = user?.role?.toLowerCase() === 'admin' || user?.role?.toLowerCase() === 'canteen';

  const initialFormData = {
    name: '',
    description: '',
    price: '',
    category: 'main',
    dietaryTags: [] as string[],
    allergens: [] as string[],
    available: true,
    preparationTime: 15,
    maxOrdersPerDay: 50
  };

  const [formData, setFormData] = useState(initialFormData);

  useEffect(() => {
    fetchMenuItems();
    fetchCategories();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedCategory, showAllItems]);

  useEffect(() => {
    localStorage.setItem('cart', JSON.stringify(cart));
  }, [cart]);

  const fetchMenuItems = async () => {
    try {
      setIsLoading(true);
      setError('');
      const response = await menuAPI.getMenuItems({
        category: selectedCategory || undefined,
        available: showAllItems ? undefined : true
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

  const handleCreateMenuItem = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setIsLoading(true);
      const data = {
        ...formData,
        price: parseFloat(formData.price),
        dietaryTags: formData.dietaryTags.filter(tag => tag.trim() !== ''),
        allergens: formData.allergens.filter(tag => tag.trim() !== '')
      };
      await menuAPI.createMenuItem(data);
      setMessage('Menu item created successfully!');
      setShowAddModal(false);
      setFormData(initialFormData);
      fetchMenuItems();
    } catch (err: any) {
      const errorMsg = err.response?.data?.message || err.message || 'Failed to create menu item';
      setError(`Failed to create menu item: ${errorMsg}`);
    } finally {
      setIsLoading(false);
    }
  };

  const handleUpdateMenuItem = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingItem) return;

    try {
      setIsLoading(true);
      const data = {
        ...formData,
        price: parseFloat(formData.price),
        dietaryTags: formData.dietaryTags.filter(tag => tag.trim() !== ''),
        allergens: formData.allergens.filter(tag => tag.trim() !== '')
      };
      await menuAPI.updateMenuItem(editingItem.id, data);
      setMessage('Menu item updated successfully!');
      setShowEditModal(false);
      setEditingItem(null);
      setFormData(initialFormData);
      fetchMenuItems();
    } catch (err: any) {
      const errorMsg = err.response?.data?.message || err.message || 'Failed to update menu item';
      setError(`Failed to update menu item: ${errorMsg}`);
    } finally {
      setIsLoading(false);
    }
  };

  const handleDeleteMenuItem = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this menu item?')) return;

    try {
      setIsLoading(true);
      await menuAPI.deleteMenuItem(id);
      setMessage('Menu item deleted successfully!');
      fetchMenuItems();
    } catch (err: any) {
      const errorMsg = err.response?.data?.message || err.message || 'Failed to delete menu item';
      setError(`Failed to delete menu item: ${errorMsg}`);
    } finally {
      setIsLoading(false);
    }
  };

  const openEditModal = (item: MenuItem) => {
    setEditingItem(item);
    setFormData({
      name: item.name,
      description: item.description || '',
      price: item.price.toString(),
      category: item.category,
      dietaryTags: item.dietaryTags || [],
      allergens: item.allergens || [],
      available: item.available,
      preparationTime: item.preparationTime || 15,
      maxOrdersPerDay: item.maxOrdersPerDay || 50
    });
    setShowEditModal(true);
  };

  const closeModal = () => {
    setShowAddModal(false);
    setShowEditModal(false);
    setEditingItem(null);
    setFormData(initialFormData);
    setError('');
  };

  const handleTagInput = (value: string, field: 'dietaryTags' | 'allergens') => {
    const tags = value.split(',').map(tag => tag.trim()).filter(tag => tag);
    setFormData({ ...formData, [field]: tags });
  };

  const filteredItems = menuItems.filter(item =>
    item.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    item.description?.toLowerCase().includes(searchQuery.toLowerCase())
  );

  if (isLoading && menuItems.length === 0) {
    return <div className="loading">Loading menu...</div>;
  }

  return (
    <div className="menu-container">
      <div className="menu-header">
        <h1>{isAdmin ? 'Menu Management 🍽️' : 'Our Menu 🍽️'}</h1>

        {isAdmin && (
          <p className="admin-badge">Welcome, {user?.name} ({user?.role})</p>
        )}

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

        {isAdmin && (
          <div className="admin-controls">
            <button
              className="add-item-btn"
              onClick={() => setShowAddModal(true)}
            >
              + Add New Menu Item
            </button>
            <label className="show-all-toggle">
              <input
                type="checkbox"
                checked={showAllItems}
                onChange={(e) => setShowAllItems(e.target.checked)}
              />
              Show all items (including unavailable)
            </label>
          </div>
        )}
      </div>

      {!isAdmin && cart.length > 0 && (
        <div className="cart-preview">
          <span>🛒 Cart: {cart.reduce((sum, c) => sum + c.quantity, 0)} items</span>
          <span>₹{cart.reduce((sum, c) => sum + (c.menuItem.price * c.quantity), 0).toFixed(2)}</span>
          <a href="/cart" className="checkout-btn">Checkout →</a>
        </div>
      )}

      {message && (
        <div className="success-message" onClick={() => setMessage('')}>
          {message}
        </div>
      )}

      {error && (
        <div className="error-message" onClick={() => setError('')}>
          {error}
        </div>
      )}

      <div className="menu-grid">
        {filteredItems.length === 0 ? (
          <div className="no-items">
            {isAdmin ? 'No menu items found. Click "Add New Menu Item" to create one.' : 'No items found matching your search.'}
          </div>
        ) : (
          filteredItems.map(item => (
            <div key={item.id} className={`menu-card ${!item.available ? 'unavailable-item' : ''}`}>
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

                {item.allergens?.length > 0 && (
                  <div className="allergens">
                    <small>⚠️ Contains: {item.allergens.join(', ')}</small>
                  </div>
                )}

                <div className="menu-meta">
                  <span>⏱️ {item.preparationTime} min</span>
                  <span>📦 Max {item.maxOrdersPerDay}/day</span>
                </div>

                <div className="menu-card-footer">
                  <span className={`availability ${item.available ? 'available' : 'unavailable'}`}>
                    {item.available ? '✓ Available' : '✗ Unavailable'}
                  </span>

                  {isAdmin ? (
                    <div className="admin-actions">
                      <button
                        className="edit-btn"
                        onClick={() => openEditModal(item)}
                      >
                        ✏️ Edit
                      </button>
                      <button
                        className="delete-btn"
                        onClick={() => handleDeleteMenuItem(item.id)}
                      >
                        🗑️ Delete
                      </button>
                    </div>
                  ) : (
                    <button
                      onClick={() => addToCart(item)}
                      disabled={!item.available}
                      className="add-btn"
                    >
                      Add to Cart
                    </button>
                  )}
                </div>
              </div>
            </div>
          ))
        )}
      </div>

      {/* Add Menu Item Modal */}
      {showAddModal && (
        <div className="modal-overlay" onClick={closeModal}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <h2>Add New Menu Item</h2>
            <form onSubmit={handleCreateMenuItem}>
              <div className="form-group">
                <label>Name *</label>
                <input
                  type="text"
                  value={formData.name}
                  onChange={(e) => setFormData({...formData, name: e.target.value})}
                  required
                />
              </div>

              <div className="form-group">
                <label>Description</label>
                <textarea
                  value={formData.description}
                  onChange={(e) => setFormData({...formData, description: e.target.value})}
                  rows={3}
                />
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label>Price (₹) *</label>
                  <input
                    type="number"
                    step="0.01"
                    min="0"
                    value={formData.price}
                    onChange={(e) => setFormData({...formData, price: e.target.value})}
                    required
                  />
                </div>

                <div className="form-group">
                  <label>Category *</label>
                  <select
                    value={formData.category}
                    onChange={(e) => setFormData({...formData, category: e.target.value})}
                  >
                    <option value="main">Main</option>
                    <option value="beverage">Beverage</option>
                    <option value="dessert">Dessert</option>
                    <option value="snack">Snack</option>
                  </select>
                </div>
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label>Prep Time (min)</label>
                  <input
                    type="number"
                    value={formData.preparationTime}
                    onChange={(e) => setFormData({...formData, preparationTime: parseInt(e.target.value)})}
                  />
                </div>

                <div className="form-group">
                  <label>Max Orders/Day</label>
                  <input
                    type="number"
                    value={formData.maxOrdersPerDay}
                    onChange={(e) => setFormData({...formData, maxOrdersPerDay: parseInt(e.target.value)})}
                  />
                </div>
              </div>

              <div className="form-group">
                <label>Dietary Tags (comma-separated)</label>
                <input
                  type="text"
                  value={formData.dietaryTags.join(', ')}
                  onChange={(e) => handleTagInput(e.target.value, 'dietaryTags')}
                  placeholder="e.g., vegetarian, vegan, gluten-free"
                />
              </div>

              <div className="form-group">
                <label>Allergens (comma-separated)</label>
                <input
                  type="text"
                  value={formData.allergens.join(', ')}
                  onChange={(e) => handleTagInput(e.target.value, 'allergens')}
                  placeholder="e.g., gluten, dairy, nuts"
                />
              </div>

              <div className="form-group checkbox">
                <label>
                  <input
                    type="checkbox"
                    checked={formData.available}
                    onChange={(e) => setFormData({...formData, available: e.target.checked})}
                  />
                  Available for ordering
                </label>
              </div>

              <div className="modal-actions">
                <button type="button" className="cancel-btn" onClick={closeModal}>
                  Cancel
                </button>
                <button type="submit" className="submit-btn" disabled={isLoading}>
                  {isLoading ? 'Creating...' : 'Create Menu Item'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Edit Menu Item Modal */}
      {showEditModal && editingItem && (
        <div className="modal-overlay" onClick={closeModal}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <h2>Edit Menu Item</h2>
            <form onSubmit={handleUpdateMenuItem}>
              <div className="form-group">
                <label>Name *</label>
                <input
                  type="text"
                  value={formData.name}
                  onChange={(e) => setFormData({...formData, name: e.target.value})}
                  required
                />
              </div>

              <div className="form-group">
                <label>Description</label>
                <textarea
                  value={formData.description}
                  onChange={(e) => setFormData({...formData, description: e.target.value})}
                  rows={3}
                />
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label>Price (₹) *</label>
                  <input
                    type="number"
                    step="0.01"
                    min="0"
                    value={formData.price}
                    onChange={(e) => setFormData({...formData, price: e.target.value})}
                    required
                  />
                </div>

                <div className="form-group">
                  <label>Category *</label>
                  <select
                    value={formData.category}
                    onChange={(e) => setFormData({...formData, category: e.target.value})}
                  >
                    <option value="main">Main</option>
                    <option value="beverage">Beverage</option>
                    <option value="dessert">Dessert</option>
                    <option value="snack">Snack</option>
                  </select>
                </div>
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label>Prep Time (min)</label>
                  <input
                    type="number"
                    value={formData.preparationTime}
                    onChange={(e) => setFormData({...formData, preparationTime: parseInt(e.target.value)})}
                  />
                </div>

                <div className="form-group">
                  <label>Max Orders/Day</label>
                  <input
                    type="number"
                    value={formData.maxOrdersPerDay}
                    onChange={(e) => setFormData({...formData, maxOrdersPerDay: parseInt(e.target.value)})}
                  />
                </div>
              </div>

              <div className="form-group">
                <label>Dietary Tags (comma-separated)</label>
                <input
                  type="text"
                  value={formData.dietaryTags.join(', ')}
                  onChange={(e) => handleTagInput(e.target.value, 'dietaryTags')}
                  placeholder="e.g., vegetarian, vegan, gluten-free"
                />
              </div>

              <div className="form-group">
                <label>Allergens (comma-separated)</label>
                <input
                  type="text"
                  value={formData.allergens.join(', ')}
                  onChange={(e) => handleTagInput(e.target.value, 'allergens')}
                  placeholder="e.g., gluten, dairy, nuts"
                />
              </div>

              <div className="form-group checkbox">
                <label>
                  <input
                    type="checkbox"
                    checked={formData.available}
                    onChange={(e) => setFormData({...formData, available: e.target.checked})}
                  />
                  Available for ordering
                </label>
              </div>

              <div className="modal-actions">
                <button type="button" className="cancel-btn" onClick={closeModal}>
                  Cancel
                </button>
                <button type="submit" className="submit-btn" disabled={isLoading}>
                  {isLoading ? 'Updating...' : 'Update Menu Item'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default Menu;
