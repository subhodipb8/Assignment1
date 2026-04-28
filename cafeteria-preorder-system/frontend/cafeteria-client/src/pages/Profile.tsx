import React, { useState, useEffect } from 'react';
import { userAPI } from '../services/api';
import { useAuth } from '../contexts/AuthContext';
import './Profile.css';

const Profile: React.FC = () => {
  const { user, refreshUser } = useAuth();
  const [walletBalance, setWalletBalance] = useState(0);
  const [preferences, setPreferences] = useState({
    dietaryPreferences: [] as string[],
    allergies: [] as string[]
  });
  const [addAmount, setAddAmount] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [message, setMessage] = useState({ type: '', text: '' });

  const dietaryOptions = ['vegetarian', 'vegan', 'gluten-free', 'dairy-free', 'halal', 'kosher'];

  useEffect(() => {
    fetchUserData();
  }, []);

  const fetchUserData = async () => {
    try {
      const [walletRes, prefsRes] = await Promise.all([
        userAPI.getWalletBalance(),
        userAPI.getPreferences()
      ]);
      setWalletBalance(walletRes.data.balance);
      setPreferences(prefsRes.data);
    } catch (err) {
      console.error('Failed to fetch user data');
    }
  };

  const handleAddFunds = async () => {
    const amount = parseFloat(addAmount);
    if (isNaN(amount) || amount <= 0) {
      setMessage({ type: 'error', text: 'Please enter a valid amount' });
      return;
    }

    setIsLoading(true);
    try {
      await userAPI.addFunds(amount);
      setMessage({ type: 'success', text: `₹${amount.toFixed(2)} added to your wallet` });
      setAddAmount('');
      fetchUserData();
      refreshUser();
    } catch (err: any) {
      setMessage({ type: 'error', text: err.response?.data?.message || 'Failed to add funds' });
    } finally {
      setIsLoading(false);
    }
  };

  const handlePreferenceChange = async () => {
    setIsLoading(true);
    try {
      await userAPI.updatePreferences(preferences);
      setMessage({ type: 'success', text: 'Preferences updated successfully' });
      refreshUser();
    } catch (err: any) {
      setMessage({ type: 'error', text: 'Failed to update preferences' });
    } finally {
      setIsLoading(false);
    }
  };

  const toggleDietaryPreference = (pref: string) => {
    const newPrefs = preferences.dietaryPreferences.includes(pref)
      ? preferences.dietaryPreferences.filter(p => p !== pref)
      : [...preferences.dietaryPreferences, pref];
    setPreferences({ ...preferences, dietaryPreferences: newPrefs });
  };

  return (
    <div className="profile-container">
      <h1>My Profile 👤</h1>

      {message.text && (
        <div className={`message ${message.type}`}>
          {message.text}
          <button onClick={() => setMessage({ type: '', text: '' })} className="close-btn">×</button>
        </div>
      )}

      <div className="profile-grid">
        <div className="profile-card">
          <h2>Personal Information</h2>
          <div className="info-group">
            <label>Name</label>
            <div className="info-value">{user?.name}</div>
          </div>
          <div className="info-group">
            <label>Email</label>
            <div className="info-value">{user?.email}</div>
          </div>
          <div className="info-group">
            <label>Role</label>
            <div className="info-value">
              {user?.role ? user.role.charAt(0).toUpperCase() + user.role.slice(1) : 'Unknown'}
            </div>
          </div>
        </div>

        <div className="profile-card wallet-card">
          <h2>💳 Wallet</h2>
          <div className="wallet-balance">
            <span className="balance-label">Current Balance</span>
            <span className="balance-amount">₹{walletBalance.toFixed(2)}</span>
          </div>

          <div className="add-funds">
            <label>Add Funds</label>
            <div className="add-funds-row">
              <input
                type="number"
                value={addAmount}
                onChange={(e) => setAddAmount(e.target.value)}
                placeholder="Amount"
                min="1"
                max="10000"
              />
              <button
                onClick={handleAddFunds}
                disabled={isLoading}
                className="add-btn"
              >
                {isLoading ? 'Adding...' : 'Add'}
              </button>
            </div>
          </div>
        </div>

        <div className="profile-card preferences-card">
          <h2>🍽️ Dietary Preferences</h2>
          <div className="preferences-section">
            <h3>Dietary Tags</h3>
            <div className="preferences-grid">
              {dietaryOptions.map(pref => (
                <label key={pref} className="preference-option">
                  <input
                    type="checkbox"
                    checked={preferences.dietaryPreferences.includes(pref)}
                    onChange={() => toggleDietaryPreference(pref)}
                  />
                  <span>{pref.charAt(0).toUpperCase() + pref.slice(1)}</span>
                </label>
              ))}
            </div>
          </div>

          <div className="preferences-section">
            <h3>Allergies</h3>
            <div className="allergies-input">
              <input
                type="text"
                value={preferences.allergies.join(', ')}
                onChange={(e) => setPreferences({
                  ...preferences,
                  allergies: e.target.value.split(',').map(a => a.trim()).filter(Boolean)
                })}
                placeholder="Enter allergies separated by commas"
              />
              <p className="help-text">Example: peanuts, shellfish, dairy, gluten</p>
            </div>
          </div>

          <button
            onClick={handlePreferenceChange}
            disabled={isLoading}
            className="save-btn"
          >
            {isLoading ? 'Saving...' : 'Save Preferences'}
          </button>
        </div>
      </div>
    </div>
  );
};

export default Profile;
