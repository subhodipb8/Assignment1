# AI Usage Log and Reflection Report

## Project: Cafeteria Food Pre-ordering System

---

## AI Tools Used

### Primary Tool: Claude (Claude Code)
- Used throughout the entire development process
- Assisted with code generation, architecture design, debugging, and documentation

---

## Development Log

### Phase 1: Project Initialization (Date: April 28, 2026)

**Prompts Used:**
1. "Read this document file line by line and give me list of work items"
   - **Purpose:** Extract assignment requirements from the FSAD document
   - **Result:** Successfully identified all deliverables and work items

2. "Donot create the sample reference School Equipment Lending Portal instead create something else"
   - **Purpose:** Pivot to an alternative problem statement
   - **Result:** Selected Cafeteria Food Pre-ordering System (Option D)

### Phase 2: Backend Development

**Prompts Used:**
3. "create the backend in .NET using postgres as DB"
   - **Purpose:** Set up .NET Web API with PostgreSQL
   - **Result:** Created ASP.NET Core project with EF Core and Npgsql

4. "Create models for User, MenuItem, and Order entities"
   - **AI Generated:**
     - User.cs with authentication fields, dietary preferences, wallet
     - MenuItem.cs with nutrition info and dietary tags
     - Order.cs with status tracking and order items
   - **Manual Adjustments:** Added PostgreSQL array types for dietary tags

5. "Set up JWT authentication with middleware"
   - **AI Generated:**
     - JwtService.cs with token generation
     - AuthController.cs with login/register endpoints
     - Program.cs configuration
   - **Manual Adjustments:** Adjusted JWT expiry time to 2 hours

6. "Create CRUD controllers for Menu and Orders"
   - **AI Generated:**
     - MenuController.cs with filtering and seeding
     - OrdersController.cs with status workflow
     - UsersController.cs for wallet management
   - **Manual Adjustments:** Added additional validation and error handling

### Phase 3: Frontend Development

**Prompts Used:**
7. "Create React frontend with TypeScript"
   - **Purpose:** Initialize React project with required packages
   - **Result:** Created React app with axios and react-router-dom

8. "Build authentication pages (Login, Register) with form validation"
   - **AI Generated:**
     - Login.tsx with email/password validation
     - Register.tsx with dietary preferences selection
     - AuthContext.tsx for global state management
   - **Manual Adjustments:** Added demo account information

9. "Create Menu page with category filters and search"
   - **AI Generated:**
     - Menu.tsx with grid layout
     - Category filtering buttons
     - Cart preview with localStorage persistence
   - **Manual Adjustments:** Added pickup time slot selection

10. "Build Cart and Checkout flow"
    - **AI Generated:**
      - Cart.tsx with order summary
      - Wallet balance checking
      - Pickup date/time selection
    - **Manual Adjustments:** Added allergen warnings

11. "Create Orders page with status tracking"
    - **AI Generated:**
      - Orders.tsx with order list
      - Status badges with color coding
      - Cancel order functionality
    - **Manual Adjustments:** Added admin status update buttons

12. "Build Admin Panel with dashboard"
    - **AI Generated:**
      - AdminPanel.tsx with statistics cards
      - Menu seeding functionality
    - **Manual Adjustments:** Added quick links to menu and orders

### Phase 4: Documentation

**Prompts Used:**
13. "Create README with project documentation"
    - **AI Generated:** Complete README.md with setup instructions

14. "Generate AI Usage Log"
    - **AI Generated:** This document

### Phase 5: Microservices Architecture Conversion (Date: May 7, 2026)

**Prompts Used:**
15. "Read the document FSAD_Assignment_2026.docx - requires microservices architecture"
    - **Purpose:** Understand assignment requirement for microservices
    - **Result:** Confirmed need for API Gateway, multiple services, separate databases

16. "Convert monolithic backend to microservices with API Gateway"
    - **AI Generated:**
      - API Gateway with Ocelot (Port 5000)
      - Auth Service (Port 5001) with own database
      - Menu Service (Port 5002) with own database
      - Order Service (Port 5003) with own database
      - UserContextHandler for claims forwarding
    - **Result:** Complete microservices architecture with proper separation

17. "Set up Ocelot routing configuration"
    - **AI Generated:** ocelot.json with routes for all services
    - **Features:** JWT validation at gateway, claims forwarding as headers

18. "Create UserContextHandler for passing user claims to downstream services"
    - **AI Generated:** DelegatingHandler that extracts JWT claims and adds X-User-Id, X-User-Role headers
    - **Purpose:** Services can identify users without re-validating tokens

19. "Update Architecture.md documentation for microservices"
    - **AI Generated:** Complete architecture diagram and service descriptions
    - **Manual Adjustments:** Added deployment architecture section

20. "Update README with microservices setup instructions"
    - **AI Generated:** Updated README with separate service startup instructions

---

## Parts Generated by AI vs Manually Coded

### AI Generated (≈85%)
- **Backend:**
  - All entity models (original + microservices)
  - DbContext configuration (separate per service)
  - JWT service and authentication
  - API controllers (Auth, Menu, Orders, Users)
  - DTOs
  - **Microservices:**
    - API Gateway with Ocelot
    - UserContextHandler for claims forwarding
    - Auth Service (separate project)
    - Menu Service (separate project)
    - Order Service (separate project)

- **Frontend:**
  - All React components (Home, Login, Register, Menu, Cart, Orders, Profile, AdminPanel)
  - AuthContext for state management
  - API service layer
  - TypeScript types
  - CSS styling for all components
  - ProtectedRoute component
  - Navbar component

- **Documentation:**
  - README.md
  - Architecture.md with microservices diagrams
  - This AI Usage Log

### Manually Coded (≈15%)
- Connection string configuration (4 databases)
- Demo account credentials setup
- Specific validation rules adjustments
- CORS policy configuration
- Minor UI text and layout tweaks

---

## Reflection

### Did AI help or hinder understanding?

**Helped:**
1. **Rapid Prototyping:** Generated complete CRUD APIs and React components in minutes, allowing focus on logic rather than boilerplate
2. **Best Practices:** Suggested proper patterns like Repository pattern, JWT auth flow, and React hooks usage
3. **Error Handling:** Provided comprehensive try-catch blocks and validation
4. **Documentation:** Generated clear, structured documentation instantly

**Areas for Improvement:**
1. **Configuration Nuances:** Had to manually adjust JWT settings and CORS for specific requirements
2. **Database-Specific Features:** PostgreSQL array types required manual attention
3. **Business Logic:** Some workflow specifics (order status transitions) needed manual refinement

### Issues Encountered Integrating AI Output

1. **Entity Framework Configuration**
   - Issue: PostgreSQL array types not automatically configured
   - Resolution: Manually added `HasColumnType("text[]")` for array fields

2. **JWT Token Validation**
   - Issue: Initial token validation failing
   - Resolution: Adjusted TokenValidationParameters to match frontend expectations

3. **CORS Configuration**
   - Issue: Frontend requests blocked by CORS
   - Resolution: Explicitly configured CORS policy in Program.cs

### What I Learned from Debugging AI-Generated Code

1. **Configuration Matters:** AI generates standard patterns, but environment-specific configs (connection strings, CORS origins) need attention
2. **Database Compatibility:** Different databases handle types differently - PostgreSQL arrays vs other databases
3. **Security Considerations:** JWT secrets must be properly secured and configured
4. **State Management:** Understanding how React Context works is crucial when debugging auth flows

### Overall Assessment

**Productivity Impact:** Significantly positive. Reduced development time by approximately 60-70%.

**Code Quality:** Good overall. Generated code followed conventions, included error handling, and was well-structured.

**Learning Value:** Moderate. While AI handled implementation, understanding the generated code was essential for debugging and customization.

**Recommendation:** AI tools are excellent for accelerating development and generating boilerplate, but developer oversight and understanding remain critical for production-ready applications.

### Microservices Conversion Reflection

**Why Microservices?**
The assignment specifically required microservices architecture. Converting from monolithic to microservices demonstrated:
- Service boundary identification
- Database-per-service pattern
- API Gateway pattern for routing
- Claims-based authentication across services

**Benefits of the New Architecture:**
1. **Independent Deployment:** Each service can be updated independently
2. **Scalability:** Scale high-traffic services (Orders) separately from others
3. **Technology Flexibility:** Each service could use different tech stacks
4. **Fault Isolation:** Menu service failure doesn't affect Orders

**Challenges Encountered:**
1. **Claims Forwarding:** Needed UserContextHandler to pass JWT claims as headers
2. **Database Management:** Three separate databases require separate migrations
3. **Development Complexity:** More services to start during development
4. **Port Management:** Each service needs its own port

**Microservices Best Practices Applied:**
- Database per service (no shared database)
- API Gateway for unified entry point
- Stateless services (authentication handled at gateway)
- Independent service deployment

---

## Conclusion

The AI-assisted development process proved highly effective for this project. Claude accelerated the development timeline while maintaining code quality. The key success factors were:

1. Clear prompts with specific requirements
2. Iterative refinement of AI-generated code
3. Manual verification of critical components (authentication, payments)
4. Understanding the generated code for effective debugging

This project demonstrates how AI can be a powerful development partner when used appropriately, handling routine coding tasks while allowing developers to focus on architecture and business logic.
