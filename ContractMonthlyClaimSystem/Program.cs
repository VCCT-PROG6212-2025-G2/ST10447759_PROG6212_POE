using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ContractMonthlyClaimSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. DB Connection
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<CMCSContext>(options =>
                options.UseSqlServer(connectionString));

            // 2. Identity (Login/Roles)
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<CMCSContext>()
                .AddDefaultTokenProviders();

            // 3. Sessions (Requirement)
            builder.Services.AddSession();

            builder.Services.AddControllersWithViews();
            builder.Services.AddSingleton<FileEncryptionService>();

            var app = builder.Build();

            // 4. Initialize DB and Create Roles (HR, Lecturer, etc.)
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<CMCSContext>();

                // --- THE FIX IS HERE ---
                // This line creates the database tables (AspNetRoles, AspNetUsers, Claims, etc.)
                // If this is commented out, you get "Invalid object name 'AspNetRoles'"
                context.Database.EnsureCreated();
                // -----------------------

                // Now it is safe to seed data because the tables exist
                await DbSeeder.SeedRolesAndAdminAsync(services);
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication(); // Check who they are
            app.UseAuthorization();  // Check what they can do
            app.UseSession();        // Enable sessions

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}"); // Change default to Login

            app.Run();
        }
    }
}