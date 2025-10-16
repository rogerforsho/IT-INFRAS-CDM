using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using CDM.InventorySystem.Models;
using CDM.InventorySystem.Services;

namespace CDM.InventorySystem.Pages.Inventory
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IItemService _itemService;

        public List<Item> Items { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string Category { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "name";

        public IndexModel(IItemService itemService)
        {
            _itemService = itemService;
        }

        public async Task OnGetAsync()
        {
            var allItems = await _itemService.GetAllItemsAsync();

            // Apply filters
            var filteredItems = allItems.AsQueryable();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                filteredItems = filteredItems.Where(i =>
                    i.ItemName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    i.BarcodeId.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    i.Brand.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    i.Model.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(Category))
            {
                filteredItems = filteredItems.Where(i => i.Category == Category);
            }

            // Apply sorting
            Items = SortBy switch
            {
                "name_desc" => filteredItems.OrderByDescending(i => i.ItemName).ToList(),
                "stock_asc" => filteredItems.OrderBy(i => i.CurrentStock).ToList(),
                "stock_desc" => filteredItems.OrderByDescending(i => i.CurrentStock).ToList(),
                _ => filteredItems.OrderBy(i => i.ItemName).ToList()
            };
        }
    }
}