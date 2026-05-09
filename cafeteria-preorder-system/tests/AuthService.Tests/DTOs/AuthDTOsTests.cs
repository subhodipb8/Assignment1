using Xunit;
using AuthService.DTOs;
using FluentAssertions;

namespace AuthService.Tests.DTOs;

public class AuthDTOsTests
{
    #region RegisterRequest Tests

    [Fact]
    public void RegisterRequest_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var request = new RegisterRequest();

        // Assert
        request.Name.Should().BeEmpty();
        request.Email.Should().BeEmpty();
        request.Password.Should().BeEmpty();
        request.Role.Should().Be("student");
    }

    [Fact]
    public void RegisterRequest_CanBeFullyPopulated()
    {
        // Arrange & Act
        var request = new RegisterRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "securepassword123",
            Role = "admin"
        };

        // Assert
        request.Name.Should().Be("John Doe");
        request.Email.Should().Be("john@example.com");
        request.Password.Should().Be("securepassword123");
        request.Role.Should().Be("admin");
    }

    #endregion

    #region LoginRequest Tests

    [Fact]
    public void LoginRequest_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var request = new LoginRequest();

        // Assert
        request.Email.Should().BeEmpty();
        request.Password.Should().BeEmpty();
    }

    [Fact]
    public void LoginRequest_CanBeFullyPopulated()
    {
        // Arrange & Act
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "mypassword"
        };

        // Assert
        request.Email.Should().Be("user@example.com");
        request.Password.Should().Be("mypassword");
    }

    #endregion

    #region AuthResponse Tests

    [Fact]
    public void AuthResponse_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var response = new AuthResponse();

        // Assert
        response.Token.Should().BeEmpty();
        response.User.Should().NotBeNull();
    }

    [Fact]
    public void AuthResponse_CanBeFullyPopulated()
    {
        // Arrange & Act
        var response = new AuthResponse
        {
            Token = "jwt-token-123",
            User = new UserDto
            {
                Id = 1,
                Name = "John",
                Email = "john@example.com",
                Role = "student"
            }
        };

        // Assert
        response.Token.Should().Be("jwt-token-123");
        response.User.Id.Should().Be(1);
        response.User.Name.Should().Be("John");
    }

    #endregion

    #region UserDto Tests

    [Fact]
    public void UserDto_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var user = new UserDto();

        // Assert
        user.Id.Should().Be(0);
        user.Name.Should().BeEmpty();
        user.Email.Should().BeEmpty();
        user.Role.Should().BeEmpty();
        user.WalletBalance.Should().Be(0);
        user.DietaryPreferences.Should().BeNull();
        user.Allergies.Should().BeNull();
        user.CreatedAt.Should().Be(default);
    }

    [Fact]
    public void UserDto_CanBeFullyPopulated()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddDays(-1);

        // Act
        var user = new UserDto
        {
            Id = 1,
            Name = "Jane Doe",
            Email = "jane@example.com",
            Role = "staff",
            WalletBalance = 250.00m,
            DietaryPreferences = new[] { "vegan", "organic" },
            Allergies = new[] { "dairy", "eggs" },
            CreatedAt = createdAt
        };

        // Assert
        user.Id.Should().Be(1);
        user.Name.Should().Be("Jane Doe");
        user.Email.Should().Be("jane@example.com");
        user.Role.Should().Be("staff");
        user.WalletBalance.Should().Be(250.00m);
        user.DietaryPreferences.Should().ContainInOrder("vegan", "organic");
        user.Allergies.Should().ContainInOrder("dairy", "eggs");
        user.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void UserDto_WithEmptyArrays_WorksCorrectly()
    {
        // Arrange & Act
        var user = new UserDto
        {
            DietaryPreferences = Array.Empty<string>(),
            Allergies = Array.Empty<string>()
        };

        // Assert
        user.DietaryPreferences.Should().BeEmpty();
        user.Allergies.Should().BeEmpty();
    }

    #endregion

    #region UpdateWalletRequest Tests

    [Fact]
    public void UpdateWalletRequest_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var request = new UpdateWalletRequest();

        // Assert
        request.Amount.Should().Be(0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10.50)]
    [InlineData(1000)]
    [InlineData(-50)]
    [InlineData(0.01)]
    public void UpdateWalletRequest_AcceptsVariousAmounts(decimal amount)
    {
        // Arrange & Act
        var request = new UpdateWalletRequest { Amount = amount };

        // Assert
        request.Amount.Should().Be(amount);
    }

    #endregion

    #region UpdatePreferencesRequest Tests

    [Fact]
    public void UpdatePreferencesRequest_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var request = new UpdatePreferencesRequest();

        // Assert
        request.DietaryPreferences.Should().BeNull();
        request.Allergies.Should().BeNull();
    }

    [Fact]
    public void UpdatePreferencesRequest_CanSetDietaryPreferences()
    {
        // Arrange & Act
        var request = new UpdatePreferencesRequest
        {
            DietaryPreferences = new[] { "vegetarian", "gluten-free" }
        };

        // Assert
        request.DietaryPreferences.Should().ContainInOrder("vegetarian", "gluten-free");
        request.Allergies.Should().BeNull();
    }

    [Fact]
    public void UpdatePreferencesRequest_CanSetAllergies()
    {
        // Arrange & Act
        var request = new UpdatePreferencesRequest
        {
            Allergies = new[] { "peanuts", "tree-nuts" }
        };

        // Assert
        request.DietaryPreferences.Should().BeNull();
        request.Allergies.Should().ContainInOrder("peanuts", "tree-nuts");
    }

    [Fact]
    public void UpdatePreferencesRequest_CanSetBoth()
    {
        // Arrange & Act
        var request = new UpdatePreferencesRequest
        {
            DietaryPreferences = new[] { "vegan" },
            Allergies = new[] { "soy" }
        };

        // Assert
        request.DietaryPreferences.Should().ContainSingle().Which.Should().Be("vegan");
        request.Allergies.Should().ContainSingle().Which.Should().Be("soy");
    }

    [Fact]
    public void UpdatePreferencesRequest_WithEmptyArrays_WorksCorrectly()
    {
        // Arrange & Act
        var request = new UpdatePreferencesRequest
        {
            DietaryPreferences = Array.Empty<string>(),
            Allergies = Array.Empty<string>()
        };

        // Assert
        request.DietaryPreferences.Should().BeEmpty();
        request.Allergies.Should().BeEmpty();
    }

    #endregion
}
