using ContractMonthlyClaimSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ContractMonthlyClaimSystem.Controllers
{
    [Authorize(Roles = "HR")] // Security Requirement
    public class HRController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public HRController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            // Generate Reports functionality can go here
            return View();
        }

        [HttpGet]
        public IActionResult AddLecturer()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddLecturer(string firstName, string lastName, string email, string password, decimal hourlyRate)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                HourlyRate = hourlyRate,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Lecturer");
                ViewBag.Message = "Lecturer added successfully!";
            }
            else
            {
                foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            }

            return View();
        }
    }
}