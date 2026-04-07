namespace ClothingStoreApp.ViewModels
{
    public class AdminAuditLogViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
