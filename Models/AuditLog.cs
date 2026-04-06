using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClothingStoreApp.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty; // e.g., "Created Order", "Updated Profile"

        [Required]
        [StringLength(500)]
        public string Details { get; set; } = string.Empty; // e.g., "User X created Order #15"

        public DateTime Timestamp { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string? EntityType { get; set; } // e.g., "Order", "User"

        public int? EntityId { get; set; } // ID of the affected entity
    }
}