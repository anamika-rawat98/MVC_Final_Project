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
    [Route("Admin/Users/[action]/{id?}")]
    public class AdminUsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AuditLogService _auditLogService;

        public AdminUsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AuditLogService auditLogService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
            var model = new List<AdminUserIndexViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new AdminUserIndexViewModel
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Roles = roles.Any() ? string.Join(", ", roles) : "No Role",
                    IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
                    IsAdmin = roles.Contains("Admin")
                });
            }

            return View("~/Views/Admin/Users/Index.cshtml", model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new AdminUserCreateViewModel();
            await PopulateRolesAsync(model.Roles);
            return View("~/Views/Admin/Users/Create.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminUserCreateViewModel model)
        {
            if (!await _roleManager.RoleExistsAsync(model.SelectedRole))
            {
                ModelState.AddModelError(nameof(model.SelectedRole), "Selected role does not exist.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateRolesAsync(model.Roles);
                return View("~/Views/Admin/Users/Create.cshtml", model);
            }

            var user = new ApplicationUser
            {
                Name = model.Name,
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                AddErrors(result);
                await PopulateRolesAsync(model.Roles);
                return View("~/Views/Admin/Users/Create.cshtml", model);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, model.SelectedRole);
            if (!roleResult.Succeeded)
            {
                AddErrors(roleResult);
                await PopulateRolesAsync(model.Roles);
                return View("~/Views/Admin/Users/Create.cshtml", model);
            }

            await LogAdminActionAsync(
                "Created User",
                $"Created user {user.Email} with role {model.SelectedRole}.",
                "User");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var model = new AdminUserEditViewModel
            {
                Id = user.Id,
                Name = user.Name ?? string.Empty,
                Email = user.Email ?? string.Empty,
                SelectedRole = userRoles.FirstOrDefault() ?? string.Empty,
                IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow
            };

            await PopulateRolesAsync(model.Roles);
            return View("~/Views/Admin/Users/Edit.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminUserEditViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            if (!await _roleManager.RoleExistsAsync(model.SelectedRole))
            {
                ModelState.AddModelError(nameof(model.SelectedRole), "Selected role does not exist.");
            }

            if (!ModelState.IsValid)
            {
                model.IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow;
                await PopulateRolesAsync(model.Roles);
                return View("~/Views/Admin/Users/Edit.cshtml", model);
            }

            user.Name = model.Name;
            user.Email = model.Email;
            user.UserName = model.Email;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                AddErrors(updateResult);
                model.IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow;
                await PopulateRolesAsync(model.Roles);
                return View("~/Views/Admin/Users/Edit.cshtml", model);
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeRolesResult.Succeeded)
                {
                    AddErrors(removeRolesResult);
                    model.IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow;
                    await PopulateRolesAsync(model.Roles);
                    return View("~/Views/Admin/Users/Edit.cshtml", model);
                }
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, model.SelectedRole);
            if (!addRoleResult.Succeeded)
            {
                AddErrors(addRoleResult);
                model.IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow;
                await PopulateRolesAsync(model.Roles);
                return View("~/Views/Admin/Users/Edit.cshtml", model);
            }

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                if (!resetResult.Succeeded)
                {
                    AddErrors(resetResult);
                    model.IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow;
                    await PopulateRolesAsync(model.Roles);
                    return View("~/Views/Admin/Users/Edit.cshtml", model);
                }
            }

            await LogAdminActionAsync(
                "Updated User",
                $"Updated user {user.Email} and assigned role {model.SelectedRole}.",
                "User");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
            await _userManager.UpdateAsync(user);
            await LogAdminActionAsync("Locked User", $"Locked user {user.Email}.", "User");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);
            await LogAdminActionAsync("Unlocked User", $"Unlocked user {user.Email}.", "User");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var model = new AdminUserDeleteViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Roles = roles.Any() ? string.Join(", ", roles) : "No Role"
            };

            return View("~/Views/Admin/Users/Delete.cshtml", model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                AddErrors(result);
                var roles = await _userManager.GetRolesAsync(user);
                var model = new AdminUserDeleteViewModel
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Roles = roles.Any() ? string.Join(", ", roles) : "No Role"
                };

                return View("~/Views/Admin/Users/Delete.cshtml", model);
            }

            await LogAdminActionAsync("Deleted User", $"Deleted user {user.Email}.", "User");

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateRolesAsync(List<SelectListItem> roles)
        {
            roles.Clear();
            var roleNames = await _roleManager.Roles.OrderBy(r => r.Name).Select(r => r.Name!).ToListAsync();
            foreach (var roleName in roleNames)
            {
                roles.Add(new SelectListItem(roleName, roleName));
            }
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
