using Xunit;
using System.ComponentModel.DataAnnotations;
using AuthService.Models;
using FluentAssertions;

namespace AuthService.Tests.Models;

public class UserTests
{
    [Fact]
    public void User_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        user.Id.Should().Be(0);
        user.Name.Should().BeEmpty();
        user.Email.Should().BeEmpty();
        user.PasswordHash.Should().BeEmpty();
        user.Role.Should().Be("student");
        user.WalletBalance.Should().Be(0);
        user.DietaryPreferences.Should().BeNull();
        user.Allergies.Should().BeNull();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void User_CanBeFullyPopulated()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddDays(-1);

        // Act
        var user = new User
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            PasswordHash = "hashedpassword123",
            Role = "admin",
            WalletBalance = 150.50m,
            DietaryPreferences = new[] { "vegetarian", "low-carb" },
            Allergies = new[] { "peanuts", "shellfish" },
            CreatedAt = createdAt
        };

        // Assert
        user.Id.Should().Be(1);
        user.Name.Should().Be("John Doe");
        user.Email.Should().Be("john@example.com");
        user.PasswordHash.Should().Be("hashedpassword123");
        user.Role.Should().Be("admin");
        user.WalletBalance.Should().Be(150.50m);
        user.DietaryPreferences.Should().ContainInOrder("vegetarian", "low-carb");
        user.Allergies.Should().ContainInOrder("peanuts", "shellfish");
        user.CreatedAt.Should().Be(createdAt);
    }

    [Theory]
    [InlineData("student")]
    [InlineData("staff")]
    [InlineData("admin")]
    [InlineData("canteen")]
    public void User_AcceptsValidRoles(string role)
    {
        // Arrange & Act
        var user = new User { Role = role };

        // Assert
        user.Role.Should().Be(role);
    }

    [Fact]
    public void User_Name_HasMaxLength100()
    {
        // Arrange
        var user = new User();
        var nameProperty = typeof(User).GetProperty("Name")!;
        var maxLengthAttribute = nameProperty.GetCustomAttributes(typeof(MaxLengthAttribute), false)
            .Cast<MaxLengthAttribute>()
            .First();

        // Assert
        maxLengthAttribute.Length.Should().Be(100);
    }

    [Fact]
    public void User_Email_HasMaxLength100AndEmailAddress()
    {
        // Arrange
        var user = new User();
        var emailProperty = typeof(User).GetProperty("Email")!;
        var attributes = emailProperty.GetCustomAttributes(false);

        // Assert
        attributes.Should().ContainSingle(a => a is MaxLengthAttribute);
        attributes.Should().ContainSingle(a => a is EmailAddressAttribute);

        var maxLength = attributes.OfType<MaxLengthAttribute>().First();
        maxLength.Length.Should().Be(100);
    }

    [Fact]
    public void User_Role_HasMaxLength20()
    {
        // Arrange
        var user = new User();
        var roleProperty = typeof(User).GetProperty("Role")!;
        var maxLengthAttribute = roleProperty.GetCustomAttributes(typeof(MaxLengthAttribute), false)
            .Cast<MaxLengthAttribute>()
            .First();

        // Assert
        maxLengthAttribute.Length.Should().Be(20);
    }

    [Fact]
    public void User_Properties_AreRequired()
    {
        // Arrange
        var userType = typeof(User);

        // Assert
        userType.GetProperty("Name")!.GetCustomAttributes(typeof(RequiredAttribute), false)
            .Should().HaveCount(1);
        userType.GetProperty("Email")!.GetCustomAttributes(typeof(RequiredAttribute), false)
            .Should().HaveCount(1);
        userType.GetProperty("PasswordHash")!.GetCustomAttributes(typeof(RequiredAttribute), false)
            .Should().HaveCount(1);
        userType.GetProperty("Role")!.GetCustomAttributes(typeof(RequiredAttribute), false)
            .Should().HaveCount(1);
    }

    [Fact]
    public void User_WalletBalance_CanBeNegative()
    {
        // Arrange & Act
        var user = new User { WalletBalance = -50m };

        // Assert
        user.WalletBalance.Should().Be(-50m);
    }

    [Fact]
    public void User_WithEmptyArrays_WorksCorrectly()
    {
        // Arrange & Act
        var user = new User
        {
            DietaryPreferences = Array.Empty<string>(),
            Allergies = Array.Empty<string>()
        };

        // Assert
        user.DietaryPreferences.Should().BeEmpty();
        user.Allergies.Should().BeEmpty();
    }

    [Fact]
    public void User_WithNullPreferences_WorksCorrectly()
    {
        // Arrange & Act
        var user = new User
        {
            DietaryPreferences = null,
            Allergies = null
        };

        // Assert
        user.DietaryPreferences.Should().BeNull();
        user.Allergies.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(999999.99)]
    [InlineData(-999999.99)]
    public void User_WalletBalance_AcceptsVariousAmounts(decimal amount)
    {
        // Arrange & Act
        var user = new User { WalletBalance = amount };

        // Assert
        user.WalletBalance.Should().Be(amount);
    }
}
