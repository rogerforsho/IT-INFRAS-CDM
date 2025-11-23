using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CDM.InventorySystem.Data;
using CDM.InventorySystem.Models;

namespace CDM.InventorySystem.Pages.Items
{
    [Authorize(Roles = "Admin")]  // Only Admin can delete
    public class DeleteModel : PageModel
    {
        private readonly InventoryDbContext _context;

        public DeleteModel(InventoryDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Item Item { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Item = await _context.Items.FindAsync(id);

            if (Item == null)
            {
                return NotFound();
            }

            // Check if item has active transactions
            var hasActiveTransactions = await _context.Transactions
                .AnyAsync(t => t.ItemId == id && t.Status == TransactionStatus.CheckedOut);

            if (hasActiveTransactions)
            {
                ErrorMessage = "Cannot delete this item because it has active check-outs. Please check in all items first.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Item = await _context.Items.FindAsync(id);

            if (Item != null)
            {
                // Check for active transactions
                var hasActiveTransactions = await _context.Transactions
                    .AnyAsync(t => t.ItemId == id && t.Status == TransactionStatus.CheckedOut);

                if (hasActiveTransactions)
                {
                    TempData["ErrorMessage"] = "Cannot delete item with active check-outs!";
                    return RedirectToPage("/Inventory/Index");
                }

                _context.Items.Remove(Item);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Item '{Item.ItemName}' deleted successfully!";
            }

            return RedirectToPage("/Inventory/Index");
        }
    }
}
