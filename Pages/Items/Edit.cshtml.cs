using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using CDM.InventorySystem.Models;
using CDM.InventorySystem.Data;
using CDM.InventorySystem.Services;
using CDM.InventorySystem.Utilities;

namespace CDM.InventorySystem.Pages.Items
{
    [Authorize(Roles = "Admin,Staff")]
    public class EditModel : PageModel
    {
        private readonly IItemService _itemService;
        private readonly ILogger<EditModel> _logger;
        private readonly InventoryDbContext _context;

        [BindProperty]
        public Item Item { get; set; } = new Item();

        public string BarcodeImage { get; set; } = string.Empty;

        public EditModel(IItemService itemService, ILogger<EditModel> logger, InventoryDbContext context)
        {
            _itemService = itemService;
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            Item = item;
            BarcodeImage = BarcodeGenerator.GenerateBarcodeImage(Item.BarcodeId);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Regenerate barcode image for display
                BarcodeImage = BarcodeGenerator.GenerateBarcodeImage(Item.BarcodeId);
                return Page();
            }

            try
            {
                var existingItem = await _context.Items.FindAsync(Item.ItemId);
                if (existingItem == null)
                {
                    return NotFound();
                }

                // Update only the properties that should be modified
                existingItem.ItemName = Item.ItemName;
                existingItem.Description = Item.Description;
                existingItem.Category = Item.Category;
                existingItem.Brand = Item.Brand;
                existingItem.Model = Item.Model;
                existingItem.Specification = Item.Specification;
                existingItem.CurrentStock = Item.CurrentStock;
                existingItem.TotalStock = Item.TotalStock;
                existingItem.MinStockLevel = Item.MinStockLevel;
                existingItem.Location = Item.Location;
                existingItem.Condition = Item.Condition;
                existingItem.Vendor = Item.Vendor;
                existingItem.PurchaseDate = Item.PurchaseDate;
                existingItem.PurchasePrice = Item.PurchasePrice;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _context.Items.AnyAsync(e => e.ItemId == Item.ItemId);
                if (!exists)
                {
                    return NotFound();
                }
                throw;
            }

            // Redirect to index page on successful update
            return RedirectToPage("/Inventory/Index");
        }
    }
}