using Microsoft.EntityFrameworkCore;
using CafeteriaAPI.Models;

namespace CafeteriaAPI.Data
{
    public static class DbSeeder
    {
        public static void SeedData(CafeteriaDbContext context)
        {
            // Ensure database is created
            context.Database.EnsureCreated();

            // Seed admin user if not exists
            if (!context.Users.Any(u => u.Email == "admin@cafeteria.com"))
            {
                var adminUser = new User
                {
                    Name = "Administrator",
                    Email = "admin@cafeteria.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = "admin",
                    WalletBalance = 0,
                    DietaryPreferences = new List<string>(),
                    Allergies = new List<string>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.Users.Add(adminUser);
                context.SaveChanges();
            }

            // Seed canteen staff user if not exists
            if (!context.Users.Any(u => u.Email == "canteen@cafeteria.com"))
            {
                var canteenUser = new User
                {
                    Name = "Canteen Manager",
                    Email = "canteen@cafeteria.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("canteen123"),
                    Role = "canteen",
                    WalletBalance = 0,
                    DietaryPreferences = new List<string>(),
                    Allergies = new List<string>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.Users.Add(canteenUser);
                context.SaveChanges();
            }

            // Seed student user if not exists
            if (!context.Users.Any(u => u.Email == "student@cafeteria.com"))
            {
                var studentUser = new User
                {
                    Name = "Demo Student",
                    Email = "student@cafeteria.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("student123"),
                    Role = "student",
                    WalletBalance = 500,
                    DietaryPreferences = new List<string> { "vegetarian" },
                    Allergies = new List<string>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.Users.Add(studentUser);
                context.SaveChanges();
            }
        }
    }
}
