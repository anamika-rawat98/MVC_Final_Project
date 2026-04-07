using ClothingStoreApp.Models;
using ClothingStoreApp.Services;
using ClothingStoreApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClothingStoreApp.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuditLogService _auditLogService;

        public OrdersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            AuditLogService auditLogService)
        {
            _context = context;
            _userManager = userManager;
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderListItemViewModel
                {
                    Id = o.Id,
                    ProductName = o.ProductName,
                    Price = o.Price,
                    Quantity = o.Quantity,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    Notes = o.Notes
                })
                .ToListAsync();

            return View(orders);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new OrderCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderCreateViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var order = new Order
            {
                UserId = userId,
                ProductName = model.ProductName,
                Price = model.Price,
                Quantity = model.Quantity,
                Notes = model.Notes,
                OrderDate = DateTime.Now,
                Status = OrderStatuses.Pending
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                userId,
                "Created Order",
                $"Created Order #{order.Id} for {order.ProductName} with status {order.Status}.",
                "Order",
                order.Id);

            TempData["OrderSuccess"] = $"Order #{order.Id} was created successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
