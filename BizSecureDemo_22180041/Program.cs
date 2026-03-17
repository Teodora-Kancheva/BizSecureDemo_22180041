using BizSecureDemo_22180041.Data;
using BizSecureDemo_22180041.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

//var builder = WebApplication.CreateBuilder(args);
//
//builder.Services.AddControllersWithViews();
//
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(
//        builder.Configuration.GetConnectionString("Default")));
//
//builder.Services.AddSingleton<PasswordHasher<AppUser>>();
//
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        options.LoginPath = "/Account/Login";
//        options.LogoutPath = "/Account/Logout";
//        options.Cookie.HttpOnly = true;
//        options.Cookie.SameSite = SameSiteMode.Lax;
//        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
//        options.SlidingExpiration = true;
//    });
//
//builder.Services.AddAuthorization();
//
//var app = builder.Build();
//
////using (var scope = app.Services.CreateScope())
////{
////    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
////    db.Database.EnsureCreated();
////}
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//    db.Database.Migrate();
//}
//
//app.UseStaticFiles();
//app.UseRouting();
//
//app.UseAuthentication();
//app.UseAuthorization();
//
//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");
//
//app.Run();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(opt =>
opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddSingleton<PasswordHasher<AppUser>>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
.AddCookie(o =>
{
    o.LoginPath = "/Account/Login";
    o.LogoutPath = "/Account/Logout";
    o.Cookie.HttpOnly = true;
    o.Cookie.SameSite = SameSiteMode.Lax;
    o.ExpireTimeSpan = TimeSpan.FromMinutes(30);
});

builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", o =>
    {
        o.PermitLimit = 5; // 5 attempts
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueLimit = 0;
    });
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
app.UseStaticFiles();
app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
name: "default",
pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
name: "login",
pattern: "Account/Login",
defaults: new { controller = "Account", action = "Login" })
.RequireRateLimiting("login");

app.Run();