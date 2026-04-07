using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClothingStoreApp.Models
{
    public static class OrderStatuses
    {
        public const string Pending = "Pending";
        public const string Processing = "Processing";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";

        public static readonly string[] All =
        {
            Pending,
            Processing,
            Completed,
            Cancelled
        };
    }

    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required]
        [StringLength(100)]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public int Quantity { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = OrderStatuses.Pending;

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
