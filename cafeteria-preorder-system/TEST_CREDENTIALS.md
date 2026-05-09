# Cafeteria Pre-order System - Test Credentials & Data

## Overview

This document contains test credentials and sample data for testing the Cafeteria Pre-order System.

---

## Test Users

### 1. Student User

| Field | Value |
|-------|-------|
| **Name** | Test Student |
| **Email** | student@test.com |
| **Password** | Student123! |
| **Role** | student |
| **User ID** | 6 |
| **Wallet Balance** | $100.00 |
| **Dietary Preferences** | vegetarian, gluten-free |
| **Allergies** | peanuts, shellfish |

**JWT Token:**
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI2IiwidW5pcXVlX25hbWUiOiJUZXN0IFN0dWRlbnQiLCJlbWFpbCI6InN0dWRlbnRAdGVzdC5jb20iLCJyb2xlIjoic3R1ZGVudCIsIm5iZiI6MTc3ODMxODczOCwiZXhwIjoxNzc4MzI1OTM4LCJpYXQiOjE3NzgzMTg3MzgsImlzcyI6IkNhZmV0ZXJpYUF1dGgiLCJhdWQiOiJDYWZldGVyaWFVc2VycyJ9.qq8DO1V4-zg_apt9E9YHGCzTWQoo9QzMs4ecbRGZ2wc
```

**Orders Created:**
- Order #9: Margherita Pizza + 2x Orange Juice ($16.97) - Status: **confirmed**
- Order #10: Chicken Caesar Salad ($10.99) - Status: **preparing**

---

### 2. Admin User

| Field | Value |
|-------|-------|
| **Name** | Test Admin |
| **Email** | admin@test.com |
| **Password** | Admin123! |
| **Role** | admin |
| **User ID** | 7 |
| **Wallet Balance** | $200.00 |

**JWT Token:**
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI3IiwidW5pcXVlX25hbWUiOiJUZXN0IEFkbWluIiwiZW1haWwiOiJhZG1pbkB0ZXN0LmNvbSIsInJvbGUiOiJhZG1pbiIsIm5iZiI6MTc3ODMxODczOCwiZXhwIjoxNzc4MzI1OTM4LCJpYXQiOjE3NzgzMTg3MzgsImlzcyI6IkNhZmV0ZXJpYUF1dGgiLCJhdWQiOiJDYWZldGVyaWFVc2VycyJ9.m6wvNB0lgJahjYMr2p3chUfZtyEvB1IQCCVmeHKRWrA
```

**Permissions:**
- Can view all orders (not just own)
- Can update order status for any order
- Can cancel any order
- Can access order statistics

---

### 3. Canteen Staff User

| Field | Value |
|-------|-------|
| **Name** | Canteen Staff |
| **Email** | canteen@test.com |
| **Password** | Canteen123! |
| **Role** | canteen |
| **User ID** | 8 |
| **Wallet Balance** | $0.00 |

**JWT Token:**
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI4IiwidW5pcXVlX25hbWUiOiJDYW50ZWVuIFN0YWZmIiwiZW1haWwiOiJjYW50ZWVuQHRlc3QuY29tIiwicm9sZSI6ImNhbnRlZW4iLCJuYmYiOjE3ODMxODczOSwiZXhwIjoxNzc4MzI1OTM5LCJpYXQiOjE3NzgzMTg3MzksImlzcyI6IkNhZmV0ZXJpYUF1dGgiLCJhdWQiOiJDYWZldGVyaWFVc2VycyJ9.QNpEk6_HGU1km0ejD96n70p5DBMJgAgTSJ3-FRnQgwc
```

---

### 4. Staff User

| Field | Value |
|-------|-------|
| **Name** | Test Staff |
| **Email** | staff@test.com |
| **Password** | Staff123! |
| **Role** | staff |
| **User ID** | 9 |
| **Wallet Balance** | $0.00 |

**JWT Token:**
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI5IiwidW5pcXVlX25hbWUiOiJUZXN0IFN0YWZmIiwiZW1haWwiOiJzdGFmZkB0ZXN0LmNvbSIsInJvbGUiOiJzdGFmZiIsIm5iZiI6MTc3ODMxODczOSwiZXhwIjoxNzc4MzI1OTM5LCJpYXQiOjE3NzgzMTg3MzksImlzcyI6IkNhZmV0ZXJpYUF1dGgiLCJhdWQiOiJDYWZldGVyaWFVc2VycyJ9.Gs0krGLsH093raG0l_96fVa4oeroRUJj_qHl0vJJ77w
```

---

## Sample Menu Items

| ID | Name | Price | Category | Dietary Tags | Allergens |
|----|------|-------|----------|--------------|-----------|
| 1 | Margherita Pizza | $8.99 | main | vegetarian | gluten, dairy |
| 2 | Chicken Caesar Salad | $10.99 | main | high-protein | dairy, eggs |
| 3 | Vegan Buddha Bowl | $9.99 | main | vegan, gluten-free | - |
| 4 | Fresh Orange Juice | $3.99 | beverage | vegan, gluten-free | - |

---

## Order Statistics

```
Total Orders:      10
Pending Orders:     1
Confirmed Orders:   3
Preparing Orders:   3
Ready Orders:       1
Completed Orders:   2
Cancelled Orders:   0
Total Revenue:   $20.98
```

---

## Quick API Tests

### Login as Student
```bash
curl -X POST http://localhost:5001/api/login \
  -H "Content-Type: application/json" \
  -d '{"email": "student@test.com", "password": "Student123!"}'
```

### Get Student Wallet
```bash
curl http://localhost:5001/api/users/wallet \
  -H "X-User-Id: 6"
```

### Get All Menu Items
```bash
curl http://localhost:5002/api/menu
```

### Create Order (as Student)
```bash
curl -X POST http://localhost:5003/api/orders \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 6" \
  -d '{
    "items": [{"menuItemId": 1, "menuItemName": "Margherita Pizza", "quantity": 1, "price": 8.99}],
    "pickupTime": "2026-05-09T14:00:00Z",
    "pickupDate": "2026-05-09T14:00:00Z"
  }'
```

### Update Order Status (as Admin)
```bash
curl -X PUT http://localhost:5003/api/orders/9/status \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 7" \
  -d '{"status": "completed"}'
```

---

## Service URLs

| Service | URL | Swagger |
|---------|-----|---------|
| Frontend | http://localhost:3000 | - |
| API Gateway | http://localhost:5000 | /swagger |
| Auth Service | http://localhost:5001 | /swagger |
| Menu Service | http://localhost:5002 | /swagger |
| Order Service | http://localhost:5003 | /swagger |

---

## Using JWT Tokens in Swagger UI

1. Open any Swagger UI (e.g., http://localhost:5001/swagger)
2. Click the **Authorize** button
3. Enter: `Bearer {your-jwt-token}`
   - Example: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`
4. Click **Authorize**
5. Click **Close**
6. All subsequent requests will include the JWT token

---

*Created: 2026-05-09*
