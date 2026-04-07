using System.ComponentModel.DataAnnotations;

namespace ClothingStoreApp.ViewModels
{
    public class OrderCreateViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 100000)]
        public decimal Price { get; set; }

        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
