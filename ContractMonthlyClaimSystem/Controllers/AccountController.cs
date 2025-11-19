using ContractMonthlyClaimSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // Check role to redirect to appropriate dashboard
                    var user = await _userManager.FindByEmailAsync(email);
                    if (await _userManager.IsInRoleAsync(user, "HR"))
                    {
                        return RedirectToAction("Index", "HR");
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Lecturer"))
                    {
                        return RedirectToAction("MyClaims", "Lecturer");
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Coordinator"))
                    {
                        return RedirectToAction("Index", "Coordinator");
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Manager"))
                    {
                        return RedirectToAction("Index", "Manager");
                    }

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View();
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}