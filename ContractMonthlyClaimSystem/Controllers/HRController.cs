using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContractMonthlyClaimSystem.Controllers
{
    [Authorize(Roles = "HR")]
    public class HRController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CMCSContext _context;
        private readonly RoleManager<IdentityRole> _roleManager; // Added to fetch roles

        public HRController(UserManager<ApplicationUser> userManager, CMCSContext context, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        // --- 1. ADD USER (ANY ROLE) ---
        [HttpGet]
        public IActionResult AddUser()
        {
            // We will just hardcode the roles in the View for simplicity
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(string firstName, string lastName, string email, string phoneNumber, decimal hourlyRate, string password, string role)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email and Password are required.");
                return View();
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                // Only Lecturers get paid; force 0 for others to prevent errors
                HourlyRate = (role == "Lecturer") ? hourlyRate : 0,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
                ViewBag.Message = $"{role} {firstName} {lastName} added successfully!";
                return View();
            }
            else
            {
                foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            }

            return View();
        }

        // GET: /HR/ManageUsers
        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var model = new List<UserWithRoleViewModel>();

            foreach (var user in users)
            {
                // Fetch the role for this specific user
                var roles = await _userManager.GetRolesAsync(user);
                var roleName = roles.FirstOrDefault() ?? "User"; // Default to "User" if no role

                model.Add(new UserWithRoleViewModel
                {
                    User = user,
                    Role = roleName
                });
            }

            return View(model);
        }

        // --- 3. EDIT USER ---
        /// <summary>
        /// Updates user details. Enforces security rule: Only Lecturers can have an Hourly Rate.
        /// Admins (HR/Manager) are forced to 0 to prevent payroll errors.
        /// </summary>
        /// <param name="id">User ID</param>
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // RULE: Check if they are a lecturer to decide if the input should be enabled
            var isLecturer = await _userManager.IsInRoleAsync(user, "Lecturer");
            ViewBag.IsLecturer = isLecturer;

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, string firstName, string lastName, string email, decimal hourlyRate)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.FirstName = firstName;
            user.LastName = lastName;
            user.Email = email;
            user.UserName = email;

            // BACKEND VALIDATION RULE: 
            // Only Lecturers can have an Hourly Rate. 
            // If they are a Coordinator, Manager, or HR, force it to 0.
            if (await _userManager.IsInRoleAsync(user, "Lecturer"))
            {
                user.HourlyRate = hourlyRate;
            }
            else
            {
                user.HourlyRate = 0;
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                ViewBag.Message = "User details updated successfully!";

                // Reset the flag for the view
                ViewBag.IsLecturer = await _userManager.IsInRoleAsync(user, "Lecturer");
                return View(user);
            }

            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);

            // Reset the flag for the view in case of error
            ViewBag.IsLecturer = await _userManager.IsInRoleAsync(user, "Lecturer");
            return View(user);
        }

        // --- 4. REPORTS ---
        [HttpGet]
        public IActionResult Reports()
        {
            var approvedClaims = _context.Claims
                .Where(c => c.Status == ClaimStatus.ManagerApproved)
                .ToList();
            return View(approvedClaims);
        }
    }
}