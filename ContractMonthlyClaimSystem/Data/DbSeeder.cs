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
            var userManager = service.GetService<UserManager<ApplicationUser>>();
            var roleManager = service.GetService<RoleManager<IdentityRole>>();

            // 1. Create Roles
            await CreateRoleAsync(roleManager, "HR");
            await CreateRoleAsync(roleManager, "Lecturer");
            await CreateRoleAsync(roleManager, "Coordinator");
            await CreateRoleAsync(roleManager, "Manager");

            // 2. Create HR Admin
            await CreateUserAsync(userManager, "hr@cmcs.com", "System", "Admin", "HR");

            // 3. Create Programme Coordinator (NEW)
            await CreateUserAsync(userManager, "coordinator@cmcs.com", "Sarah", "Coordinator", "Coordinator");

            // 4. Create Academic Manager (NEW)
            await CreateUserAsync(userManager, "manager@cmcs.com", "Mike", "Manager", "Manager");
        }

        // Helper method to create roles safely
        private static async Task CreateRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Helper method to create users safely
        private static async Task CreateUserAsync(UserManager<ApplicationUser> userManager, string email, string fName, string lName, string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                var newUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = fName,
                    LastName = lName,
                    EmailConfirmed = true,
                    HourlyRate = 0 // Admins don't claim hours
                };

                var result = await userManager.CreateAsync(newUser, "Password@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, role);
                }
            }
        }
    }
}