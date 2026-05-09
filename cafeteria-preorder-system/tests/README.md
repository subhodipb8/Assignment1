# Cafeteria Pre-order System Unit Tests

This folder contains comprehensive unit tests for all microservices in the Cafeteria Pre-order System.

## Test Projects

### 1. ApiGateway.Tests
Tests for the API Gateway including:
- `UserContextHandlerTests.cs` - Tests for the delegating handler that passes user context to downstream services

### 2. AuthService.Tests
Tests for the Authentication Service including:
- `Controllers/AuthControllerTests.cs` - Tests for authentication endpoints (register, login, get current user)
- `Controllers/UsersControllerTests.cs` - Tests for wallet and preferences management
- `Services/JwtServiceTests.cs` - Tests for JWT token generation and validation
- `Models/UserTests.cs` - Tests for the User model
- `DTOs/AuthDTOsTests.cs` - Tests for data transfer objects
- `Data/AuthDbContextTests.cs` - Tests for database context

### 3. MenuService.Tests
Tests for the Menu Service including:
- `Controllers/MenuControllerTests.cs` - Tests for menu CRUD operations, filtering, and seeding
- `Models/MenuItemTests.cs` - Tests for the MenuItem model
- `DTOs/MenuDTOsTests.cs` - Tests for data transfer objects
- `Data/MenuDbContextTests.cs` - Tests for database context

### 4. OrderService.Tests
Tests for the Order Service including:
- `Controllers/OrdersControllerTests.cs` - Tests for order management, status updates, and statistics
- `Models/OrderTests.cs` - Tests for the Order model
- `Models/OrderItemTests.cs` - Tests for the OrderItem model
- `DTOs/OrderDTOsTests.cs` - Tests for data transfer objects
- `Data/OrderDbContextTests.cs` - Tests for database context

## Running the Tests

### Run all tests:
```bash
dotnet test
```

### Run tests for a specific project:
```bash
cd AuthService.Tests && dotnet test
cd MenuService.Tests && dotnet test
cd OrderService.Tests && dotnet test
cd ApiGateway.Tests && dotnet test
```

### Run with verbose output:
```bash
dotnet test --verbosity normal
```

### Run with code coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Test Frameworks Used

- **xUnit** - Test framework
- **Moq** - Mocking framework
- **FluentAssertions** - Assertion library for readable test assertions
- **Microsoft.EntityFrameworkCore.InMemory** - In-memory database for isolated testing

## Test Structure

Each test follows the Arrange-Act-Assert pattern:

```csharp
[Fact]
public async Task TestName_DoesWhat_ExpectResult()
{
    // Arrange
    var request = new Request { /* setup */ };

    // Act
    var result = await _controller.Action(request);

    // Assert
    result.Should().BeOfType<ExpectedResultType>();
}
```

## Coverage Areas

### AuthService Tests
- User registration with validation
- User login with credentials verification
- JWT token generation and validation
- Wallet balance management
- Dietary preferences and allergies
- Database constraints and relationships

### MenuService Tests
- Menu item CRUD operations
- Filtering by category, search, and availability
- Pagination and ordering
- Database seeding
- Model validation

### OrderService Tests
- Order creation with item calculation
- Status management workflow
- Role-based access control
- Order cancellation rules
- Statistics calculation
- Database relationships

### ApiGateway Tests
- User context header propagation
- Authentication forwarding
- Claims extraction
