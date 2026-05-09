using Xunit;
using Microsoft.EntityFrameworkCore;
using AuthService.Data;
using AuthService.Models;
using FluentAssertions;

namespace AuthService.Tests.Data;

public class AuthDbContextTests : IDisposable
{
    private readonly AuthDbContext _context;

    public AuthDbContextTests()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AuthDbContext(options);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public void AuthDbContext_CanBeConstructed()
    {
        // Assert
        _context.Should().NotBeNull();
        _context.Users.Should().NotBeNull();
    }

    [Fact]
    public async Task AuthDbContext_CanAddAndRetrieveUser()
    {
        // Arrange
        var user = new User
        {
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hash123",
            Role = "student"
        };

        // Act
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Assert
        var retrievedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        retrievedUser.Should().NotBeNull();
        retrievedUser!.Name.Should().Be("Test User");
    }

    [Fact(Skip = "InMemory database does not enforce unique constraints - would require integration test with PostgreSQL")]
    public async Task AuthDbContext_UserHasUniqueEmailConstraint()
    {
        // Arrange
        var user1 = new User
        {
            Name = "User 1",
            Email = "duplicate@example.com",
            PasswordHash = "hash1",
            Role = "student"
        };

        var user2 = new User
        {
            Name = "User 2",
            Email = "duplicate@example.com",
            PasswordHash = "hash2",
            Role = "staff"
        };

        _context.Users.Add(user1);
        await _context.SaveChangesAsync();

        // Act & Assert
        _context.Users.Add(user2);
        await Assert.ThrowsAsync<DbUpdateException>(() => _context.SaveChangesAsync());
    }

    [Fact]
    public async Task AuthDbContext_CanUpdateUser()
    {
        // Arrange
        var user = new User
        {
            Name = "Original Name",
            Email = "update@example.com",
            PasswordHash = "hash",
            Role = "student",
            WalletBalance = 0
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        user.Name = "Updated Name";
        user.WalletBalance = 100m;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        // Assert
        var retrievedUser = await _context.Users.FindAsync(user.Id);
        retrievedUser!.Name.Should().Be("Updated Name");
        retrievedUser.WalletBalance.Should().Be(100m);
    }

    [Fact]
    public async Task AuthDbContext_CanDeleteUser()
    {
        // Arrange
        var user = new User
        {
            Name = "To Delete",
            Email = "delete@example.com",
            PasswordHash = "hash",
            Role = "student"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        // Assert
        var retrievedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "delete@example.com");
        retrievedUser.Should().BeNull();
    }

    [Fact]
    public async Task AuthDbContext_CanQueryWithFilters()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Name = "Admin User", Email = "admin@test.com", PasswordHash = "hash", Role = "admin" },
            new() { Name = "Student 1", Email = "student1@test.com", PasswordHash = "hash", Role = "student" },
            new() { Name = "Student 2", Email = "student2@test.com", PasswordHash = "hash", Role = "student" },
            new() { Name = "Staff User", Email = "staff@test.com", PasswordHash = "hash", Role = "staff" }
        };

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        // Act
        var students = await _context.Users.Where(u => u.Role == "student").ToListAsync();
        var admins = await _context.Users.Where(u => u.Role == "admin").ToListAsync();

        // Assert
        students.Should().HaveCount(2);
        admins.Should().HaveCount(1);
    }

    [Fact]
    public async Task AuthDbContext_CanQueryById()
    {
        // Arrange
        var user = new User
        {
            Name = "Test User",
            Email = "find@example.com",
            PasswordHash = "hash",
            Role = "student"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var foundUser = await _context.Users.FindAsync(user.Id);

        // Assert
        foundUser.Should().NotBeNull();
        foundUser!.Email.Should().Be("find@example.com");
    }

    [Fact]
    public async Task AuthDbContext_FindAsync_ReturnsNullForNonexistentId()
    {
        // Act
        var result = await _context.Users.FindAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AuthDbContext_CanAddUsersWithArrays()
    {
        // Arrange
        var user = new User
        {
            Name = "Test User",
            Email = "arrays@example.com",
            PasswordHash = "hash",
            Role = "student",
            DietaryPreferences = new[] { "vegetarian", "low-carb" },
            Allergies = new[] { "peanuts" }
        };

        // Act
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Assert
        var retrievedUser = await _context.Users.FirstAsync(u => u.Email == "arrays@example.com");
        retrievedUser.DietaryPreferences.Should().ContainInOrder("vegetarian", "low-carb");
        retrievedUser.Allergies.Should().ContainSingle().Which.Should().Be("peanuts");
    }
}
