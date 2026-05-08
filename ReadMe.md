# Cafeteria Pre-ordering System - Readme

**Duration:** 8-10 minutes
**Target:** BITS WILP Faculty Evaluation
**Presenter:** Student Name
**Project:** Cafeteria Food Pre-ordering System (Microservices Architecture)

---

## Video Structure

### **INTRO (0:00 - 0:30)**
**[Screen: Title Slide]**

**Narration:**
"Hello, this is the demonstration video for the Cafeteria Food Pre-ordering System, developed as part of the Full Stack Application Development course assignment. This system allows students and staff to pre-order meals, select pickup times, and make digital payments, while enabling canteen staff to manage orders efficiently through a microservices-based architecture."

**[Transition to Problem Statement]**

---

### **SECTION 1: Problem Statement & Solution (0:30 - 1:30)**
**[Screen: Problem Statement Slide or Diagram]**

**Narration:**
"The problem we address is the long queues and wait times during peak hours in institutional cafeterias. Students often face uncertainty about food availability and must wait in line with cash payments. Our solution provides a digital platform for pre-ordering with scheduled pickups, digital wallet payments, and real-time order tracking."

**Key Points to Highlight:**
- Long queues during lunch breaks
- Uncertainty about food availability
- Cash handling delays
- Food wastage due to over-preparation
- Solution benefits: Time-saving, convenience, efficiency

**[Transition to Architecture Overview]**

---

### **SECTION 2: Architecture Overview (1:30 - 3:00)**
**[Screen: Architecture Diagram]**

**Narration:**
"Our system follows a microservices architecture pattern. The frontend is built with React and TypeScript. All requests go through an API Gateway using Ocelot, which handles routing and authentication. Behind the gateway, we have three independent microservices:"

**Walk through each service:**
1. **Auth Service** (Port 5001): "Handles user registration, login, JWT token generation, and wallet management."
2. **Menu Service** (Port 5002): "Manages menu items, categories, dietary tags, and availability tracking."
3. **Order Service** (Port 5003): "Handles order creation, status workflow, and order statistics."

**Emphasize:**
- Database per service pattern (3 PostgreSQL databases)
- JWT validation at Gateway level
- Independent service deployment
- Claims forwarding via headers

**[Transition to Live Demo]**

---

### **SECTION 3: Application Demo - User Features (3:00 - 6:00)**

#### **3.1 Landing Page & Registration (3:00 - 3:45)**
**[Screen: Application running at localhost:3000]**

**Actions:**
1. Show homepage with hero section
2. Click "Get Started" or navigate to Register
3. Fill registration form:
   - Name: "Demo Student"
   - Email: "demo@student.com"
   - Password: "password123"
   - Role: "Student"
   - Add dietary preferences: Vegetarian
   - Add allergies: Peanuts
4. Submit registration
5. Show success message

**Narration:**
"Users start at our responsive landing page. They can register with their details, select dietary preferences like vegetarian or vegan, and specify any allergies. This information helps filter menu items later."

---

#### **3.2 Login & Dashboard (3:45 - 4:15)**
**Actions:**
1. Navigate to Login page
2. Enter credentials: "demo@student.com" / "password123"
3. Show successful login
4. Display user menu in navbar
5. Show wallet balance display

**Narration:**
"After registration, users can log in. The system uses JWT tokens for authentication, validated at the API Gateway. Upon login, users see their profile in the navigation and their digital wallet balance."

---

#### **3.3 Menu Browsing (4:15 - 4:45)**
**Actions:**
1. Navigate to Menu page
2. Show menu items grid
3. Demonstrate category filters (Main, Beverages, Snacks, etc.)
4. Use search functionality
5. Show dietary tags and allergens on items
6. Add items to cart
7. Show cart preview

**Narration:**
"The menu page displays all available items with category filters and search functionality. Each item shows dietary tags like vegetarian or gluten-free, and allergen warnings. Users can add items to their cart, which persists in local storage."

---

#### **3.4 Cart & Checkout (4:45 - 5:30)**
**Actions:**
1. Navigate to Cart page
2. Show cart items with quantities
3. Adjust quantities
4. Select pickup date (tomorrow)
5. Select pickup time (12:30 PM)
6. Add special instructions
7. Show order summary with total
8. Click "Place Order"
9. Show order confirmation

**Narration:**
"In the cart, users review their selections, choose a convenient pickup date and time, and add any special instructions. The order total is calculated automatically. Upon placing the order, the system creates the order in the Order Service and deducts the amount from the digital wallet."

---

#### **3.5 Order Tracking (5:30 - 6:00)**
**Actions:**
1. Navigate to Orders page
2. Show list of orders
3. Show order status (pending)
4. Click on order to see details
5. Show order items, pickup time, total

**Narration:**
"Users can track their orders in real-time. Each order goes through a status workflow: pending, confirmed, preparing, ready, and completed. Users can also cancel pending orders if needed."

**[Transition to Admin Features]**

---

### **SECTION 4: Admin Features (6:00 - 7:30)**

#### **4.1 Admin Login (6:00 - 6:15)**
**Actions:**
1. Logout from student account
2. Login as admin: "admin@cafeteria.com" / "admin123"
3. Show admin dashboard

**Narration:**
"Now let's see the admin capabilities. Administrators have additional privileges to manage the system."

---

#### **4.2 Admin Dashboard (6:15 - 6:45)**
**Actions:**
1. Show dashboard statistics cards
2. Point out: Total Orders, Pending Orders, Revenue
3. Show quick action buttons

**Narration:**
"The admin dashboard provides an overview of system activity, including total orders, pending orders requiring attention, and revenue metrics. This helps canteen staff plan preparation and manage operations."

---

#### **4.3 Menu Management (6:45 - 7:15)**
**Actions:**
1. Navigate to Menu Management
2. Show existing menu items
3. Click "Add New Item"
4. Fill form:
   - Name: "Grilled Chicken Wrap"
   - Price: $10.99
   - Category: "Main"
   - Dietary tags: High Protein
   - Preparation time: 12 minutes
5. Save item
6. Show item added to menu

**Narration:**
"Admins can manage the entire menu through CRUD operations. They can add new items with details like price, category, dietary tags, and preparation time. They can also update availability and set daily order limits to manage inventory."

---

#### **4.4 Order Management (7:15 - 7:30)**
**Actions:**
1. Navigate to Orders
2. Show all orders
3. Find the order placed earlier
4. Update status from "pending" to "confirmed"
5. Show status update success

**Narration:**
"Admins can view all orders and update their status through the workflow. This keeps customers informed about their order progress."

**[Transition to Technical Summary]**

---

### **SECTION 5: Technical Implementation (7:30 - 8:30)**
**[Screen: Code/IDE or Architecture Diagram]**

**Narration:**
"Let me briefly highlight the technical implementation. The backend follows a microservices architecture with .NET 10, using Entity Framework Core with PostgreSQL. Each service has its own database following the database-per-service pattern."

**Show code snippets:**
1. **Microservices Structure:** Show folder structure
2. **API Gateway:** Show ocelot.json configuration
3. **JWT Authentication:** Show token validation
4. **Database Schema:** Show entity models
5. **Frontend:** Show React component structure

**Key Points:**
- Independent service deployment
- JWT authentication at Gateway
- Database per service
- React with TypeScript frontend
- Swagger API documentation

---

### **SECTION 6: Conclusion (8:30 - 9:00)**
**[Screen: Summary Slide]**

**Narration:**
"To summarize, the Cafeteria Pre-ordering System demonstrates:
- A complete full-stack application with microservices architecture
- JWT-based authentication and role-based access control
- Real-time order tracking and management
- Digital wallet integration
- Responsive UI for both users and admins

The system successfully addresses the problem of long cafeteria queues while providing a scalable, maintainable architecture."

---

### **OUTRO (9:00 - 9:30)**
**[Screen: Thank You / Contact Info]**

**Narration:**
"Thank you for watching this demonstration. The complete source code is available in the GitHub repository linked in the submission. Feel free to reach out with any questions."

---

## Technical Notes for Recording

### **Before Recording:**
1. Start all services in order:
   ```bash
   # Terminal 1: Auth Service
   cd microservices/AuthService && dotnet run --urls "http://localhost:5001"

   # Terminal 2: Menu Service
   cd microservices/MenuService && dotnet run --urls "http://localhost:5002"

   # Terminal 3: Order Service
   cd microservices/OrderService && dotnet run --urls "http://localhost:5003"

   # Terminal 4: API Gateway
   cd microservices/ApiGateway && dotnet run --urls "http://localhost:5010"

   # Terminal 5: Frontend
   cd frontend/cafeteria-client && npm start
   ```

2. Seed sample data:
   ```bash
   curl -X POST http://localhost:5010/api/menu/seed
   ```

3. Clear browser cache/history for clean demo

4. Prepare demo accounts:
   - Student: demo@student.com / password123
   - Admin: admin@cafeteria.com / admin123

### **Screen Recording Settings:**
- Resolution: 1920x1080 or 1440x900
- Frame rate: 30fps
- Audio: Clear narration
- Cursor: Visible and highlighted

### **Post-Production:**
1. Add title cards between sections
2. Zoom in on important UI elements
3. Add captions for key features
4. Background music (optional, low volume)
5. Export in 1080p quality

---

## Demo Checklist

### **Pre-Demo:**
- [ ] All services running
- [ ] Database seeded with sample data
- [ ] Browser open at localhost:3000
- [ ] Demo accounts ready
- [ ] Screen recorder configured

### **During Demo:**
- [ ] Speak clearly and at moderate pace
- [ ] Show mouse movements
- [ ] Wait for page loads
- [ ] Highlight key features
- [ ] Show error handling (optional)

### **Post-Demo:**
- [ ] Review recording quality
- [ ] Check audio levels
- [ ] Verify all sections covered
- [ ] Upload to Google Drive
- [ ] Set sharing permissions (BITS emails)

---

**End of Script**
