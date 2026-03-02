using BizSecureDemo_22180041.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BizSecureDemo.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
        => _db = db;

  //  public async Task<IActionResult> Index()
  //  {
  //      var userId = int.Parse(
  //          User.FindFirstValue(ClaimTypes.NameIdentifier)!);
  //
  //      var orders = await _db.Orders
  //          .Where(o => o.UserId == userId)
  //          .OrderByDescending(o => o.Id)
  //          .ToListAsync();
  //
  //      return View(orders);
  //  }

    public async Task<IActionResult> Index()
    {
        var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var myOrders = await _db.Orders
            .Where(o => o.UserId == uid)
            .OrderByDescending(o => o.Id)
            .ToListAsync();

        var allOrders = await _db.Orders
            .OrderByDescending(o => o.Id)
            .ToListAsync();

        ViewBag.AllOrders = allOrders;

        return View(myOrders);
    }
}