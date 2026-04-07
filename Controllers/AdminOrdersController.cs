using ClothingStoreApp.Models;
using ClothingStoreApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClothingStoreApp.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Orders/[action]/{id?}")]
    public class AdminOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var orders = (await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync())
                .Select(o => new OrderListItemViewModel
                {
                    Id = o.Id,
                    CustomerName = string.IsNullOrWhiteSpace(o.User?.Name) ? (o.User?.Email ?? "Unknown User") : o.User.Name,
                    CustomerEmail = o.User?.Email ?? string.Empty,
                    ProductName = o.ProductName,
                    Price = o.Price,
                    Quantity = o.Quantity,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    Notes = o.Notes
                })
                .ToList();

            return View(orders);
        }
    }
}
