using ClothingStoreApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClothingStoreApp.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            await context.Database.MigrateAsync();
            await EnsureNameColumnExistsAsync(context);
            await EnsureOrderStatusColumnExistsAsync(context);

            // Create roles if they don't exist
            string[] roleNames = { "Admin", "Manager", "User" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create default admin user
            var adminEmail = "admin@clothingstore.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    Name = "System Admin",
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Create default manager user
            var managerEmail = "manager@clothingstore.com";
            var managerUser = await userManager.FindByEmailAsync(managerEmail);

            if (managerUser == null)
            {
                managerUser = new ApplicationUser
                {
                    Name = "Store Manager",
                    UserName = managerEmail,
                    Email = managerEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(managerUser, "Manager123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(managerUser, "Manager");
                }
            }

            // Create default user
            var userEmail = "user@clothingstore.com";
            var regularUser = await userManager.FindByEmailAsync(userEmail);

            if (regularUser == null)
            {
                regularUser = new ApplicationUser
                {
                    Name = "Store User",
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(regularUser, "User123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(regularUser, "User");
                }
            }
        }

        private static async Task EnsureNameColumnExistsAsync(ApplicationDbContext context)
        {
            await using var connection = context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA table_info('AspNetUsers');";

            await using var reader = await command.ExecuteReaderAsync();
            var hasNameColumn = false;
            while (await reader.ReadAsync())
            {
                if (string.Equals(reader["name"]?.ToString(), "Name", StringComparison.OrdinalIgnoreCase))
                {
                    hasNameColumn = true;
                    break;
                }
            }

            await reader.DisposeAsync();

            if (!hasNameColumn)
            {
                await context.Database.ExecuteSqlRawAsync(
                    "ALTER TABLE AspNetUsers ADD COLUMN Name TEXT NULL;");
            }
        }

        private static async Task EnsureOrderStatusColumnExistsAsync(ApplicationDbContext context)
        {
            await using var connection = context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA table_info('Orders');";

            await using var reader = await command.ExecuteReaderAsync();
            var hasStatusColumn = false;
            while (await reader.ReadAsync())
            {
                if (string.Equals(reader["name"]?.ToString(), "Status", StringComparison.OrdinalIgnoreCase))
                {
                    hasStatusColumn = true;
                    break;
                }
            }

            await reader.DisposeAsync();

            if (!hasStatusColumn)
            {
                await context.Database.ExecuteSqlRawAsync(
                    "ALTER TABLE Orders ADD COLUMN Status TEXT NOT NULL DEFAULT 'Pending';");
            }
        }
    }
}
