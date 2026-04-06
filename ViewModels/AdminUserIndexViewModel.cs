namespace ClothingStoreApp.ViewModels
{
    public class AdminUserIndexViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string Roles { get; set; } = string.Empty;
        public bool IsLockedOut { get; set; }
        public bool IsAdmin { get; set; }
    }
}
