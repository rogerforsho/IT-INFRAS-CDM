
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CDM.InventorySystem.Models;
using CDM.InventorySystem.Data;
using CDM.InventorySystem.Utilities;

namespace CDM.InventorySystem.Pages.Items
{
    public class AddModel : PageModel
    {
        [BindProperty]
        public Item Item { get; set; }

        public string BarcodeImage { get; set; }
        public string GeneratedBarcode { get; set; }

        private readonly InventoryDbContext _context;

        public AddModel(InventoryDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            GeneratedBarcode = Guid.NewGuid().ToString("N");
            Item = new Item { BarcodeId = GeneratedBarcode };
            BarcodeImage = BarcodeGenerator.GenerateBarcodeImage(GeneratedBarcode);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // ✅ Fix: Automatically set CurrentStock equal to TotalStock when adding new item
            Item.CurrentStock = Item.TotalStock;

            _context.Items.Add(Item);
            await _context.SaveChangesAsync();

            // Redirect to the inventory listing page
            return RedirectToPage("/Inventory/Index");
        }
    }
}