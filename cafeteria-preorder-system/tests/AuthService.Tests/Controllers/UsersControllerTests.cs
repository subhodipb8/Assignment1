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

public class UsersControllerTests : IDisposable
{
    private readonly AuthDbContext _context;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AuthDbContext(options);
        _jwtServiceMock = new Mock<IJwtService>();
        _controller = new UsersController(_context, _jwtServiceMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetWalletBalance_WithValidUser_ReturnsBalance()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hash",
            WalletBalance = 150.75m
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        SetupUserContext("1");

        // Act
        var result = await _controller.GetWalletBalance();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value as dynamic;
        ((decimal)value.GetType().GetProperty("balance")!.GetValue(value)!).Should().Be(150.75m);
    }

    [Fact]
    public async Task GetWalletBalance_WithoutUserId_ReturnsUnauthorized()
    {
        // Arrange
        SetupUserContext(null);

        // Act
        var result = await _controller.GetWalletBalance();

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task AddFunds_WithPositiveAmount_IncreasesBalance()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hash",
            WalletBalance = 100m
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        SetupUserContext("1");

        var request = new UpdateWalletRequest { Amount = 50m };

        // Act
        var result = await _controller.AddFunds(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value as dynamic;
        ((decimal)value.GetType().GetProperty("balance")!.GetValue(value)!).Should().Be(150m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(-0.01)]
    public async Task AddFunds_WithInvalidAmount_ReturnsBadRequest(decimal amount)
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hash",
            WalletBalance = 100m
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        SetupUserContext("1");

        var request = new UpdateWalletRequest { Amount = amount };

        // Act
        var result = await _controller.AddFunds(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AddFunds_ForNonexistentUser_ReturnsNotFound()
    {
        // Arrange
        SetupUserContext("999");

        var request = new UpdateWalletRequest { Amount = 50m };

        // Act
        var result = await _controller.AddFunds(request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeductFunds_WithSufficientBalance_DecreasesBalance()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hash",
            WalletBalance = 100m
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        SetupUserContext("1");

        var request = new UpdateWalletRequest { Amount = 30m };

        // Act
        var result = await _controller.DeductFunds(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value as dynamic;
        ((decimal)value.GetType().GetProperty("balance")!.GetValue(value)!).Should().Be(70m);
    }

    [Fact]
    public async Task DeductFunds_WithInsufficientBalance_ReturnsBadRequest()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hash",
            WalletBalance = 50m
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        SetupUserContext("1");

        var request = new UpdateWalletRequest { Amount = 100m };

        // Act
        var result = await _controller.DeductFunds(request);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().BeEquivalentTo(new { message = "Insufficient funds" });
    }

    [Fact]
    public async Task DeductFunds_WhenBalanceExactlyMatches_Succeeds()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hash",
            WalletBalance = 100m
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        SetupUserContext("1");

        var request = new UpdateWalletRequest { Amount = 100m };

        // Act
        var result = await _controller.DeductFunds(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value as dynamic;
        ((decimal)value.GetType().GetProperty("balance")!.GetValue(value)!).Should().Be(0m);
    }

    [Fact]
    public async Task GetPreferences_WithValidUser_ReturnsPreferences()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hash",
            DietaryPreferences = new[] { "vegetarian", "low-carb" },
            Allergies = new[] { "peanuts", "shellfish" }
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        SetupUserContext("1");

        // Act
        var result = await _controller.GetPreferences();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value as dynamic;
        var dietaryPrefs = value.GetType().GetProperty("dietaryPreferences")!.GetValue(value) as string[];
        var allergies = value.GetType().GetProperty("allergies")!.GetValue(value) as string[];
        dietaryPrefs.Should().Contain(new[] { "vegetarian", "low-carb" });
        allergies.Should().Contain(new[] { "peanuts", "shellfish" });
    }

    [Fact]
    public async Task GetPreferences_WithoutUserId_ReturnsUnauthorized()
    {
        // Arrange
        SetupUserContext(null);

        // Act
        var result = await _controller.GetPreferences();

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task UpdatePreferences_UpdatesAllFields()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hash",
            DietaryPreferences = new[] { "old-preference" },
            Allergies = new[] { "old-allergy" }
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        SetupUserContext("1");

        var request = new UpdatePreferencesRequest
        {
            DietaryPreferences = new[] { "vegan", "gluten-free" },
            Allergies = new[] { "dairy", "soy" }
        };

        // Act
        var result = await _controller.UpdatePreferences(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value as dynamic;
        var dietaryPrefs = value.GetType().GetProperty("dietaryPreferences")!.GetValue(value) as string[];
        var allergies = value.GetType().GetProperty("allergies")!.GetValue(value) as string[];
        dietaryPrefs.Should().Contain(new[] { "vegan", "gluten-free" });
        allergies.Should().Contain(new[] { "dairy", "soy" });
    }

    [Fact]
    public async Task UpdatePreferences_WithNullDietaryPreferences_KeepsExisting()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hash",
            DietaryPreferences = new[] { "existing" },
            Allergies = new[] { "old-allergy" }
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        SetupUserContext("1");

        var request = new UpdatePreferencesRequest
        {
            DietaryPreferences = null,
            Allergies = new[] { "new-allergy" }
        };

        // Act
        var result = await _controller.UpdatePreferences(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value as dynamic;
        var dietaryPrefs = value.GetType().GetProperty("dietaryPreferences")!.GetValue(value) as string[];
        var allergies = value.GetType().GetProperty("allergies")!.GetValue(value) as string[];
        dietaryPrefs.Should().Contain("existing");
        allergies.Should().Contain("new-allergy");
    }

    [Fact]
    public async Task UpdatePreferences_WithNullAllergies_KeepsExisting()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hash",
            DietaryPreferences = new[] { "old-diet" },
            Allergies = new[] { "existing" }
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        SetupUserContext("1");

        var request = new UpdatePreferencesRequest
        {
            DietaryPreferences = new[] { "new-diet" },
            Allergies = null
        };

        // Act
        var result = await _controller.UpdatePreferences(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value as dynamic;
        var dietaryPrefs = value.GetType().GetProperty("dietaryPreferences")!.GetValue(value) as string[];
        var allergies = value.GetType().GetProperty("allergies")!.GetValue(value) as string[];
        dietaryPrefs.Should().Contain("new-diet");
        allergies.Should().Contain("existing");
    }

    [Fact]
    public async Task UpdatePreferences_WithBothNull_KeepsBothExisting()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hash",
            DietaryPreferences = new[] { "existing-diet" },
            Allergies = new[] { "existing-allergy" }
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        SetupUserContext("1");

        var request = new UpdatePreferencesRequest
        {
            DietaryPreferences = null,
            Allergies = null
        };

        // Act
        var result = await _controller.UpdatePreferences(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value as dynamic;
        var dietaryPrefs = value.GetType().GetProperty("dietaryPreferences")!.GetValue(value) as string[];
        var allergies = value.GetType().GetProperty("allergies")!.GetValue(value) as string[];
        dietaryPrefs.Should().Contain("existing-diet");
        allergies.Should().Contain("existing-allergy");
    }

    [Fact]
    public async Task UpdatePreferences_WithEmptyArrays_SetsToEmpty()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hash",
            DietaryPreferences = new[] { "existing-diet" },
            Allergies = new[] { "existing-allergy" }
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        SetupUserContext("1");

        var request = new UpdatePreferencesRequest
        {
            DietaryPreferences = Array.Empty<string>(),
            Allergies = Array.Empty<string>()
        };

        // Act
        var result = await _controller.UpdatePreferences(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value as dynamic;
        var dietaryPrefs = value.GetType().GetProperty("dietaryPreferences")!.GetValue(value) as string[];
        var allergies = value.GetType().GetProperty("allergies")!.GetValue(value) as string[];
        dietaryPrefs.Should().BeEmpty();
        allergies.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdatePreferences_WithValidToken_UsesTokenUserId()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hash"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _controller.Request.Headers.Authorization = "Bearer valid-token";
        _jwtServiceMock.Setup(x => x.ValidateToken("valid-token")).Returns(1);

        var request = new UpdatePreferencesRequest
        {
            DietaryPreferences = new[] { "vegan" }
        };

        // Act
        var result = await _controller.UpdatePreferences(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    private void SetupUserContext(string? userId)
    {
        var httpContext = new DefaultHttpContext();
        if (userId != null)
        {
            httpContext.Request.Headers.Add("X-User-Id", userId);
        }
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }
}
