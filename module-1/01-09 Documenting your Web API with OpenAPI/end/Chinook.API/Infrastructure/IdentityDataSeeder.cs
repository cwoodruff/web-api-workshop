using Chinook.API.Identity;
using Microsoft.AspNetCore.Identity;

namespace Chinook.API.Infrastructure;

public static class IdentityDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = ["Admin", "Manager", "User"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var adminUserName = "admin";
        var adminEmail = "admin@chinook.local";
        var admin = await userManager.FindByNameAsync(adminUserName);
        if (admin == null)
        {
            admin = new ApplicationUser { UserName = adminUserName, Email = adminEmail, EmailConfirmed = true };
            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}