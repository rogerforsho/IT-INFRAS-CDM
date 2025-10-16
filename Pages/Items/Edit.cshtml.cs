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

        public async Task<IActionResult> OnGetAsync(string id)
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
                _context.Attach(Item).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _context.Items.AnyAsync(e => e.BarcodeId == Item.BarcodeId);
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