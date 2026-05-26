using LandiGlobalTemplate.Models;
using LandiGlobalTemplate.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace LandiGlobalTemplate.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly EmailService _emailService;
        private readonly IWebHostEnvironment _environment;
        private const string EmailVerificationProvider = "EmailVerification";
        private const string EmailVerificationCodeName = "Code";
        private const string EmailVerificationExpiresName = "ExpiresUtc";

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            EmailService emailService,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _environment = environment;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            await Task.CompletedTask;
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail(string? email)
        {
            await Task.CompletedTask;
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            await Task.CompletedTask;
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerificationCode(string email)
        {
            await Task.CompletedTask;
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
                    
                    if (result.Succeeded)
                    {
                        user.LastLogin = DateTime.UtcNow;
                        await _userManager.UpdateAsync(user);

                        return RedirectToLocal(returnUrl);
                    }
                    
                    if (result.IsLockedOut)
                    {
                        ModelState.AddModelError("", "Compte verrouillé suite à plusieurs tentatives échouées.");
                    }
                    else if (result.IsNotAllowed && !await _userManager.IsEmailConfirmedAsync(user))
                    {
                        ModelState.AddModelError("", "Veuillez confirmer votre email avec le code de vérification avant de vous connecter.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Email ou mot de passe invalides.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Email ou mot de passe invalides.");
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var model = new UserProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? "",
                Phone = user.PhoneNumber,
                Department = user.Department
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UserProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return NotFound();

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.Phone;
                user.Department = model.Department;

                var result = await _userManager.UpdateAsync(user);
                
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Profil mis à jour avec succès.";
                    return RedirectToAction(nameof(Profile));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        private async Task CreateAndSendVerificationCodeAsync(ApplicationUser user)
        {
            var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            var expiresUtc = DateTime.UtcNow.AddMinutes(15).ToString("O");

            await _userManager.SetAuthenticationTokenAsync(user, EmailVerificationProvider, EmailVerificationCodeName, code);
            await _userManager.SetAuthenticationTokenAsync(user, EmailVerificationProvider, EmailVerificationExpiresName, expiresUtc);

            var sent = await _emailService.SendVerificationCodeAsync(user.Email ?? "", user.FirstName ?? "", code);
            if (!sent)
            {
                TempData["WarningMessage"] = "Le code n'a pas pu être envoyé. Vérifiez la configuration SMTP dans appsettings.json.";
                if (_environment.IsDevelopment())
                {
                    TempData["DebugVerificationCode"] = code;
                }
            }
        }
    }

    // View Models
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Le prénom est obligatoire.")]
        public string FirstName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Le nom est obligatoire.")]
        public string LastName { get; set; } = string.Empty;
        [Required(ErrorMessage = "L'email est obligatoire.")]
        [EmailAddress(ErrorMessage = "Email invalide.")]
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
        public string Password { get; set; } = string.Empty;
        [Compare(nameof(Password), ErrorMessage = "Les mots de passe ne correspondent pas.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "L'email est obligatoire.")]
        [EmailAddress(ErrorMessage = "Email invalide.")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }

    public class VerifyEmailViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le code de vérification est obligatoire.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Le code doit contenir 6 chiffres.")]
        public string Code { get; set; } = string.Empty;
    }

    public class UserProfileViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Department { get; set; }
    }
}
