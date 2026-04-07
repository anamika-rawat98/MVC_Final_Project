using ClothingStoreApp.Models;

namespace ClothingStoreApp.Services
{
    public class AuditLogService
    {
        private readonly ApplicationDbContext _context;

        public AuditLogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string userId, string action, string details, string? entityType = null, int? entityId = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            _context.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                Action = action,
                Details = details,
                EntityType = entityType,
                EntityId = entityId,
                Timestamp = DateTime.Now
            });

            await _context.SaveChangesAsync();
        }
    }
}
