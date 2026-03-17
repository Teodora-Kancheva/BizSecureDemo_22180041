using System.Security.Claims;
using BizSecureDemo_22180041.Data;
using BizSecureDemo_22180041.Models;
using BizSecureDemo_22180041.ViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;

namespace BizSecureDemo.Controllers;

//public class AccountController : Controller
//{
//    private readonly AppDbContext _db;
//    private readonly PasswordHasher<AppUser> _hasher;
//
//    public AccountController(AppDbContext db, PasswordHasher<AppUser> hasher)
//    {
//        _db = db;
//        _hasher = hasher;
//    }
//
//    [HttpGet]
//    public IActionResult Register() => View(new RegisterVm());
//
//    [HttpPost]
//    [ValidateAntiForgeryToken]
//    public async Task<IActionResult> Register(RegisterVm vm)
//    {
//        if (!ModelState.IsValid)
//            return View(vm);
//
//        var email = vm.Email.Trim().ToLowerInvariant();
//
//        if (await _db.Users.AnyAsync(u => u.Email == email))
//        {
//            ModelState.AddModelError("", "Този email вече е регистриран.");
//            return View(vm);
//        }
//
//        var user = new AppUser { Email = email };
//        user.PasswordHash = _hasher.HashPassword(user,vm.Password);
//        
//
//        _db.Users.Add(user);
//        await _db.SaveChangesAsync();
//
//        return RedirectToAction(nameof(Login));
//    }
//
//    [HttpGet]
//    public IActionResult Login() => View(new LoginVm());
//

//    [HttpPost]
//    [ValidateAntiForgeryToken]
//    public async Task<IActionResult> Login(LoginVm vm)
//    {
//        if (!ModelState.IsValid)
//            return View(vm);
//
//        var email = vm.Email.Trim().ToLowerInvariant();
//
//        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
//
//        if (user == null ||
//            _hasher.VerifyHashedPassword(user, user.PasswordHash, vm.Password)
//                == PasswordVerificationResult.Failed)
//        {
//            ModelState.AddModelError("", "Грешен email или парола.");
//            return View(vm);
//        }
//
//        var claims = new List<Claim>
//        {
//            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
//            new(ClaimTypes.Name, user.Email)
//        };
//
//        var identity = new ClaimsIdentity(
//            claims,
//            CookieAuthenticationDefaults.AuthenticationScheme);
//
//        await HttpContext.SignInAsync(
//            CookieAuthenticationDefaults.AuthenticationScheme,
//            new ClaimsPrincipal(identity));
//
//        return RedirectToAction("Index", "Home");
//    }
//
//    [HttpPost]
//    [ValidateAntiForgeryToken]
//    public async Task<IActionResult> Logout()
//    {
//        await HttpContext.SignOutAsync(
//            CookieAuthenticationDefaults.AuthenticationScheme);
//
//        return RedirectToAction(nameof(Login));
//    }
//}

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<AppUser> _hasher;

    public AccountController(AppDbContext db, PasswordHasher<AppUser> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var email = vm.Email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Email == email))
        {
            ModelState.AddModelError("", "Този email вече е регистриран.");
            return View(vm);
        }

        var user = new AppUser
        {
            Email = email,
            FailedLogins = 0,
            LockoutUntilUtc = null
        };
        user.PasswordHash = _hasher.HashPassword(user, vm.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult Login() => View(new LoginVm());

    [EnableRateLimiting("login")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var email = vm.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            ModelState.AddModelError("", "Wrong email or password.");
            return View(vm);
        }

        if (user.LockoutUntilUtc != null && user.LockoutUntilUtc > DateTime.UtcNow)
        {
            ModelState.AddModelError("", "Account is temporarily locked. Try again later.");
            return View(vm);
        }

        if (_hasher.VerifyHashedPassword(user, user.PasswordHash, vm.Password) ==
            PasswordVerificationResult.Failed)
        {
            user.FailedLogins = (user.FailedLogins ?? 0) + 1;

            if (user.FailedLogins >= 5)
            {
                user.LockoutUntilUtc = DateTime.UtcNow.AddMinutes(5);
                user.FailedLogins = 0; 
            }

            await _db.SaveChangesAsync();
            ModelState.AddModelError("", "Wrong email or password.");
            return View(vm);
        }

        user.FailedLogins = 0;
        user.LockoutUntilUtc = null;
        await _db.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity)
        );

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
}