using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Application.Common.Constants;

namespace TaskManagement.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        var roleNames = new[] { Role.AdminRole, Role.UserRole };

        foreach (var role in roleNames)
        {
            var roleExists = await roleManager.RoleExistsAsync(role);

            if (!roleExists)
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }
}