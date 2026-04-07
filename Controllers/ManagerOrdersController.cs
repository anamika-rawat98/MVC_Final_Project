using ClothingStoreApp.Models;
using ClothingStoreApp.Services;
using ClothingStoreApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClothingStoreApp.Controllers
{
    [Authorize(Roles = "Manager,Admin")]
    [Route("Manager/Orders/[action]/{id?}")]
    public class ManagerOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuditLogService _auditLogService;

        public ManagerOrdersController(
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
            var model = new ManagerOrdersIndexViewModel
            {
                Orders = (await _context.Orders
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
                    .ToList(),
                StatusOptions = OrderStatuses.All
                    .Select(status => new SelectListItem(status, status))
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            if (!OrderStatuses.All.Contains(status))
            {
                TempData["OrderError"] = "Invalid order status selected.";
                return RedirectToAction(nameof(Index));
            }

            var order = await _context.Orders.Include(o => o.User).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            if (order.Status == status)
            {
                TempData["OrderSuccess"] = $"Order #{order.Id} already has status {status}.";
                return RedirectToAction(nameof(Index));
            }

            var previousStatus = order.Status;
            order.Status = status;
            await _context.SaveChangesAsync();

            var actorId = _userManager.GetUserId(User);
            if (actorId != null)
            {
                await _auditLogService.LogAsync(
                    actorId,
                    "Updated Order Status",
                    $"Changed Order #{order.Id} for {(order.User?.Email ?? "user")} from {previousStatus} to {status}.",
                    "Order",
                    order.Id);
            }

            TempData["OrderSuccess"] = $"Order #{order.Id} status changed to {status}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
