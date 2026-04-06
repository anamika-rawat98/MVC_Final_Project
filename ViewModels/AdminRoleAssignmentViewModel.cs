using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ClothingStoreApp.ViewModels
{
    public class AdminRoleAssignmentViewModel
    {
        [Required]
        [Display(Name = "User")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Role")]
        public string RoleName { get; set; } = string.Empty;

        public List<SelectListItem> Users { get; set; } = new();
        public List<SelectListItem> Roles { get; set; } = new();
        public List<AdminRoleUserAssignmentViewModel> CurrentAssignments { get; set; } = new();
    }

    public class AdminRoleUserAssignmentViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }
}
