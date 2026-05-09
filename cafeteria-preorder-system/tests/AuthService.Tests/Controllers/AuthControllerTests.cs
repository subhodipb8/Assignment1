using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthService.Controllers;
using AuthService.Data;
using AuthService.DTOs;
using AuthService.Models;
using AuthService.Services;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace AuthService.Tests.Controllers;

public class AuthControllerTests : IDisposable
{
    private readonly AuthDbContext _context;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AuthDbContext(options);
        _jwtServiceMock = new Mock<IJwtService>();
        _controller = new AuthController(_context, _jwtServiceMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Register_WithValidRequest_ReturnsOkWithToken()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "password123",
            Role = "student"
        };

        var expectedToken = "test-jwt-token";
        _jwtServiceMock.Setup(x => x.GenerateToken(It.IsAny<User>())).Returns(expectedToken);

        // Act
        var result = await _controller.Register(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AuthResponse>().Subject;
        response.Token.Should().Be(expectedToken);
        response.User.Email.Should().Be("test@example.com");
        response.User.Name.Should().Be("Test User");
        response.User.Role.Should().Be("student");
    }

    [Fact]
    public async Task Register_WithMissingFields_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Name = "",
            Email = "test@example.com",
            Password = "password123"
        };

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        // Arrange
        _context.Users.Add(new User
        {
            Name = "Existing User",
            Email = "existing@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            Role = "student"
        });
        await _context.SaveChangesAsync();

        var request = new RegisterRequest
        {
            Name = "Test User",
            Email = "existing@example.com",
            Password = "password123",
            Role = "student"
        };

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("teacher")]
    [InlineData("manager")]
    public async Task Register_WithInvalidRole_ReturnsBadRequest(string invalidRole)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "password123",
            Role = invalidRole
        };

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Theory]
    [InlineData("student")]
    [InlineData("staff")]
    [InlineData("admin")]
    [InlineData("canteen")]
    public async Task Register_WithValidRoles_ReturnsOk(string validRole)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Name = "Test User",
            Email = $"test_{validRole}@example.com",
            Password = "password123",
            Role = validRole
        };

        _jwtServiceMock.Setup(x => x.GenerateToken(It.IsAny<User>())).Returns("token");

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Register_NormalizesEmailToLowercase()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Name = "Test User",
            Email = "TEST@EXAMPLE.COM",
            Password = "password123",
            Role = "student"
        };

        _jwtServiceMock.Setup(x => x.GenerateToken(It.IsAny<User>())).Returns("token");

        // Act
        await _controller.Register(request);

        // Assert
        var user = await _context.Users.FirstAsync();
        user.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Register_SetsWalletBalanceToZero()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "password123",
            Role = "student"
        };

        _jwtServiceMock.Setup(x => x.GenerateToken(It.IsAny<User>())).Returns("token");

        // Act
        var result = await _controller.Register(request);

        // Assert
        var okResult = result.As<OkObjectResult>();
        var response = okResult.Value.As<AuthResponse>();
        response.User.WalletBalance.Should().Be(0);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var password = "password123";
        _context.Users.Add(new User
        {
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "student",
            DietaryPreferences = new[] { "vegetarian" },
            Allergies = new[] { "nuts" },
            WalletBalance = 100.50m
        });
        await _context.SaveChangesAsync();

        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = password
        };

        var expectedToken = "test-jwt-token";
        _jwtServiceMock.Setup(x => x.GenerateToken(It.IsAny<User>())).Returns(expectedToken);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AuthResponse>().Subject;
        response.Token.Should().Be(expectedToken);
        response.User.Email.Should().Be("test@example.com");
        response.User.DietaryPreferences.Should().Contain("vegetarian");
        response.User.Allergies.Should().Contain("nuts");
        response.User.WalletBalance.Should().Be(100.50m);
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "password123"
        };

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        _context.Users.Add(new User
        {
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
            Role = "student"
        });
        await _context.SaveChangesAsync();

        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_WithMissingFields_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "",
            Password = ""
        };

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_IsCaseInsensitiveForEmail()
    {
        // Arrange
        var password = "password123";
        _context.Users.Add(new User
        {
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "student"
        });
        await _context.SaveChangesAsync();

        var request = new LoginRequest
        {
            Email = "TEST@EXAMPLE.COM",
            Password = password
        };

        _jwtServiceMock.Setup(x => x.GenerateToken(It.IsAny<User>())).Returns("token");

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetCurrentUser_WithValidHeader_ReturnsUserDto()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = "student",
            DietaryPreferences = new[] { "vegan" },
            Allergies = new[] { "gluten" }
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _controller.Request.Headers.Add("X-User-Id", "1");

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var userDto = okResult.Value.Should().BeOfType<UserDto>().Subject;
        userDto.Id.Should().Be(1);
        userDto.Name.Should().Be("Test User");
        userDto.DietaryPreferences.Should().Contain("vegan");
    }

    [Fact]
    public async Task GetCurrentUser_WithoutHeader_ReturnsUnauthorized()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task GetCurrentUser_WithInvalidUserId_ReturnsNotFound()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _controller.Request.Headers.Add("X-User-Id", "999");

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetCurrentUser_WithValidToken_ReturnsUserDto()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = "student"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _controller.Request.Headers.Authorization = "Bearer valid-token";

        _jwtServiceMock.Setup(x => x.ValidateToken("valid-token")).Returns(1);

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}
