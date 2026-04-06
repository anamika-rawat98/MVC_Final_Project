using System.ComponentModel.DataAnnotations;

namespace ClothingStoreApp.ViewModels
{
    public class AdminRoleFormViewModel
    {
        public string? Id { get; set; }

        [Required]
        [StringLength(256)]
        [Display(Name = "Role Name")]
        public string Name { get; set; } = string.Empty;
    }
}
