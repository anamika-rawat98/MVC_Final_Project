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
    [Authorize(Roles = "Admin")]
    [Route("Admin/Roles/[action]/{id?}")]
    public class AdminRolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuditLogService _auditLogService;

        public AdminRolesController(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            AuditLogService auditLogService)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
            var users = await _userManager.Users.ToListAsync();
            var model = new List<AdminRoleIndexViewModel>();

            foreach (var role in roles)
            {
                var count = 0;
                foreach (var user in users)
                {
                    if (await _userManager.IsInRoleAsync(user, role.Name!))
                    {
                        count++;
                    }
                }

                model.Add(new AdminRoleIndexViewModel
                {
                    Id = role.Id,
                    Name = role.Name ?? string.Empty,
                    UserCount = count
                });
            }

            return View("~/Views/Admin/Roles/Index.cshtml", model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("~/Views/Admin/Roles/Create.cshtml", new AdminRoleFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminRoleFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/Roles/Create.cshtml", model);
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(model.Name));
            if (!result.Succeeded)
            {
                AddErrors(result);
                return View("~/Views/Admin/Roles/Create.cshtml", model);
            }

            await LogAdminActionAsync("Created Role", $"Created role {model.Name}.", "Role");
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var model = new AdminRoleFormViewModel
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty
            };

            return View("~/Views/Admin/Roles/Edit.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminRoleFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/Roles/Edit.cshtml", model);
            }

            var role = await _roleManager.FindByIdAsync(model.Id!);
            if (role == null)
            {
                return NotFound();
            }

            role.Name = model.Name;
            role.NormalizedName = model.Name.ToUpperInvariant();

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                AddErrors(result);
                return View("~/Views/Admin/Roles/Edit.cshtml", model);
            }

            await LogAdminActionAsync("Updated Role", $"Updated role to {model.Name}.", "Role");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                TempData["RoleError"] = string.Join(" ", result.Errors.Select(e => e.Description));
            }
            else
            {
                await LogAdminActionAsync("Deleted Role", $"Deleted role {role.Name}.", "Role");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Assign()
        {
            var model = new AdminRoleAssignmentViewModel();
            await PopulateAssignmentListsAsync(model);
            return View("~/Views/Admin/Roles/Assign.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(AdminRoleAssignmentViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            var roleExists = await _roleManager.RoleExistsAsync(model.RoleName);

            if (user == null)
            {
                ModelState.AddModelError(nameof(model.UserId), "Selected user does not exist.");
            }

            if (!roleExists)
            {
                ModelState.AddModelError(nameof(model.RoleName), "Selected role does not exist.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateAssignmentListsAsync(model);
                return View("~/Views/Admin/Roles/Assign.cshtml", model);
            }

            if (!await _userManager.IsInRoleAsync(user!, model.RoleName))
            {
                var result = await _userManager.AddToRoleAsync(user!, model.RoleName);
                if (!result.Succeeded)
                {
                    AddErrors(result);
                    await PopulateAssignmentListsAsync(model);
                    return View("~/Views/Admin/Roles/Assign.cshtml", model);
                }

                await LogAdminActionAsync(
                    "Assigned Role",
                    $"Assigned role {model.RoleName} to user {user!.Email}.",
                    "Role");
            }

            return RedirectToAction(nameof(Assign));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRoleFromUser(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            await _userManager.RemoveFromRoleAsync(user, roleName);
            await LogAdminActionAsync("Removed Role", $"Removed role {roleName} from user {user.Email}.", "Role");
            return RedirectToAction(nameof(Assign));
        }

        private async Task PopulateAssignmentListsAsync(AdminRoleAssignmentViewModel model)
        {
            model.Users = await _userManager.Users
                .OrderBy(u => u.Email)
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"{(string.IsNullOrWhiteSpace(u.Name) ? u.Email : u.Name)} ({u.Email})"
                })
                .ToListAsync();

            model.Roles = await _roleManager.Roles
                .OrderBy(r => r.Name)
                .Select(r => new SelectListItem
                {
                    Value = r.Name!,
                    Text = r.Name!
                })
                .ToListAsync();

            var assignments = new List<AdminRoleUserAssignmentViewModel>();
            var users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    assignments.Add(new AdminRoleUserAssignmentViewModel
                    {
                        UserId = user.Id,
                        UserName = string.IsNullOrWhiteSpace(user.Name) ? user.Email ?? string.Empty : user.Name,
                        Email = user.Email,
                        RoleName = role
                    });
                }
            }

            model.CurrentAssignments = assignments;
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private async Task LogAdminActionAsync(string action, string details, string entityType)
        {
            var adminId = _userManager.GetUserId(User);
            if (adminId != null)
            {
                await _auditLogService.LogAsync(adminId, action, details, entityType);
            }
        }
    }
}
