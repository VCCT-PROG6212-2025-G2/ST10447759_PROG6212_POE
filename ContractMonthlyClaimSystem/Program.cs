using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
// Remove InMemoryClaimService, we use DB now

var app = builder.Build();

// 4. Initialize DB and Create Roles (HR, Lecturer, etc.)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<CMCSContext>();
    // context.Database.EnsureCreated(); // Use Migrations preferably, but this works for prototypes

    // Call a seeder method here to create roles if they don't exist (Code provided below)
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