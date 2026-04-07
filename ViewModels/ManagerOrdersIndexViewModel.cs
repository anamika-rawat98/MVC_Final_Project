using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClothingStoreApp.ViewModels
{
    public class ManagerOrdersIndexViewModel
    {
        public List<OrderListItemViewModel> Orders { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
    }
}
