using ContractMonthlyClaimSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace ContractMonthlyClaimSystem.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            // Get the required services
            var userManager = service.GetService<UserManager<ApplicationUser>>();
            var roleManager = service.GetService<RoleManager<IdentityRole>>();

            // 1. Create Roles if they don't exist
            await CreateRoleAsync(roleManager, "HR");
            await CreateRoleAsync(roleManager, "Lecturer");
            await CreateRoleAsync(roleManager, "Coordinator");
            await CreateRoleAsync(roleManager, "Manager");

            // 2. Create the Admin HR User (since there is no register page)
            var adminEmail = "hr@cmcs.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Admin",
                    EmailConfirmed = true,
                    HourlyRate = 0 // HR doesn't claim hours
                };

                // Create user with a default password
                var result = await userManager.CreateAsync(newAdmin, "Password@123");

                if (result.Succeeded)
                {
                    // Assign the HR role to this user
                    await userManager.AddToRoleAsync(newAdmin, "HR");
                }
            }
        }

        private static async Task CreateRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}