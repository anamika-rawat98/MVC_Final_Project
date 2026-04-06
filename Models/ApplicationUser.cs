using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ClothingStoreApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string? Name { get; set; }
    }
}
