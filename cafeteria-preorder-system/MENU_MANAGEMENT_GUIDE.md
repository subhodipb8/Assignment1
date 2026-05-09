# Menu Management Implementation Guide

## Overview

Menu management is now role-based and restricted to **admin** and **canteen** users only.

## Access Control

| Role | View Menu | Add Items | Edit Items | Delete Items |
|------|-----------|-----------|------------|--------------|
| **Student** | ✅ | ❌ | ❌ | ❌ |
| **Staff** | ✅ | ❌ | ❌ | ❌ |
| **Canteen** | ✅ | ✅ | ✅ | ✅ |
| **Admin** | ✅ | ✅ | ✅ | ✅ |

## Backend API Changes

### Protected Endpoints (Admin/Canteen Only)

- `POST /api/menu` - Create new menu item
- `PUT /api/menu/{id}` - Update menu item
- `DELETE /api/menu/{id}` - Delete menu item
- `POST /api/menu/seed` - Seed sample data

### Public Endpoints

- `GET /api/menu` - Get all menu items
- `GET /api/menu/{id}` - Get specific menu item
- `GET /api/menu/categories` - Get all categories

### Response Codes

| Code | Meaning |
|------|---------|
| 200/201 | Success |
| 401 | Unauthorized - Authentication required |
| 403 | Forbidden - Requires admin or canteen role |
| 404 | Menu item not found |

## Frontend Changes

### Menu Page (`/menu`)

When logged in as **admin** or **canteen**:

1. **Title changes** to "Menu Management 🍽️"
2. **"+ Add New Menu Item"** button appears at top
3. **Show all items** toggle to view unavailable items
4. **Edit** button on each menu item card
5. **Delete** button on each menu item card
6. **Modal form** for adding/editing items with fields:
   - Name (required)
   - Description
   - Price (required)
   - Category (dropdown: main, beverage, dessert, snack)
   - Preparation Time
   - Max Orders Per Day
   - Dietary Tags (comma-separated)
   - Allergens (comma-separated)
   - Available checkbox

### Admin Panel (`/admin`)

Still available with:
- Dashboard statistics
- Order management
- Menu view (with seed option)

## Testing the Implementation

### Test 1: Student Cannot Create Menu Item
```bash
curl -X POST http://localhost:5002/api/menu \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 6" \
  -H "X-User-Role: student" \
  -d '{"name": "Test", "price": 10.99, "category": "main"}'
```
**Expected:** `403 Forbidden - requires admin or canteen role`

### Test 2: Canteen Can Create Menu Item
```bash
curl -X POST http://localhost:5002/api/menu \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 8" \
  -H "X-User-Role: canteen" \
  -d '{
    "name": "Canteen Special",
    "price": 12.99,
    "category": "main",
    "description": "Special from canteen",
    "available": true,
    "preparationTime": 20,
    "maxOrdersPerDay": 30
  }'
```
**Expected:** `201 Created` with menu item details

### Test 3: Admin Can Update Menu Item
```bash
curl -X PUT http://localhost:5002/api/menu/1 \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 7" \
  -H "X-User-Role: admin" \
  -d '{"price": 15.99, "available": false}'
```
**Expected:** `200 OK` with updated menu item

### Test 4: Admin Can Delete Menu Item
```bash
curl -X DELETE http://localhost:5002/api/menu/1 \
  -H "X-User-Id: 7" \
  -H "X-User-Role: admin"
```
**Expected:** `204 No Content`

## Using the Frontend

### As Canteen/Admin:

1. Login with canteen credentials:
   - Email: `canteen@test.com`
   - Password: `Canteen123!`

2. Navigate to **Menu** page

3. You will see:
   - "+ Add New Menu Item" button
   - "Show all items" checkbox
   - Edit/Delete buttons on each item

4. Click **"+ Add New Menu Item"** to create a new item

5. Click **"Edit"** on any card to modify it

6. Click **"Delete"** on any card to remove it

### As Student:

1. Login with student credentials:
   - Email: `student@test.com`
   - Password: `Student123!`

2. Navigate to **Menu** page

3. You will only see:
   - Available menu items
   - "Add to Cart" buttons
   - No management options

## Files Modified

### Backend
- `/microservices/MenuService/Controllers/MenuController.cs`
  - Added `IsAuthorized()` helper method
  - Added `HasMenuManagementRole()` helper method
  - Added `[ProducesResponseType]` attributes for 401/403 responses
  - Added authorization checks to Create, Update, Delete, and SeedData methods

### Frontend
- `/frontend/cafeteria-client/src/pages/Menu.tsx`
  - Added modal forms for Add/Edit
  - Added CRUD handlers
  - Added admin controls (add button, show all toggle)
  - Added Edit/Delete buttons on cards

- `/frontend/cafeteria-client/src/pages/Menu.css`
  - Added styles for admin controls
  - Added modal styling
  - Added form styling
  - Added responsive design

## Notes

- The API Gateway automatically forwards `X-User-Id` and `X-User-Role` headers from the JWT token
- Students and staff can still view the menu but cannot modify it
- Unavailable items are shown with reduced opacity for admin/canteen users
- Allergens are displayed with a warning icon (⚠️) on menu cards

---

*Last Updated: 2026-05-09*
