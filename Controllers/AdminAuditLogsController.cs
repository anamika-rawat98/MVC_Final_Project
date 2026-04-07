using ClothingStoreApp.Models;
using ClothingStoreApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClothingStoreApp.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/AuditLogs/[action]/{id?}")]
    public class AdminAuditLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminAuditLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var logs = (await _context.AuditLogs
                .Include(log => log.User)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync())
                .Select(log => new AdminAuditLogViewModel
                {
                    Id = log.Id,
                    UserName = string.IsNullOrWhiteSpace(log.User?.Name) ? (log.User?.Email ?? "Unknown User") : log.User.Name,
                    UserEmail = log.User?.Email ?? string.Empty,
                    Action = log.Action,
                    Details = log.Details,
                    EntityType = log.EntityType,
                    EntityId = log.EntityId,
                    Timestamp = log.Timestamp
                })
                .ToList();

            return View(logs);
        }
    }
}
