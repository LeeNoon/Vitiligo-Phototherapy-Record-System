using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitiligoTracker.Models.ViewModels;
using VitiligoTracker.Services;

namespace VitiligoTracker.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IRsaService _rsaService;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IRsaService rsaService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _rsaService = rsaService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Password))
                {
                    ModelState.AddModelError("", "密码不能为空");
                    ViewBag.PublicKey = _rsaService.GetPublicKey();
                    return View();
                }
                // Use PhoneNumber as UserName
                var user = new IdentityUser { UserName = model.PhoneNumber, PhoneNumber = model.PhoneNumber };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Default role: User
                    await _userManager.AddToRoleAsync(user, "User");

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Patients");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.PublicKey = _rsaService.GetPublicKey();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Password))
                {
                    ModelState.AddModelError("", "密码不能为空");
                    ViewBag.PublicKey = _rsaService.GetPublicKey();
                    return View(model);
                }
                // Decrypt password
                var decryptedPassword = _rsaService.Decrypt(model.Password);
                if (string.IsNullOrEmpty(decryptedPassword))
                {
                    ModelState.AddModelError(string.Empty, "密码解密失败，请刷新页面重试。");
                    ViewBag.PublicKey = _rsaService.GetPublicKey();
                    return View(model);
                }

                // Allow login with either PhoneNumber or UserName (for admin)
                // First try to find user by PhoneNumber
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);
                var userName = user != null ? user.UserName : model.PhoneNumber;

                if (string.IsNullOrEmpty(userName))
                {
                    ModelState.AddModelError("", "用户名不能为空");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(userName, decryptedPassword, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Patients");
                }

                ModelState.AddModelError(string.Empty, "登录失败，请检查手机号和密码。");
            }

            ViewBag.PublicKey = _rsaService.GetPublicKey();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
