using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CDM.InventorySystem.Data;
using CDM.InventorySystem.Models;

namespace CDM.InventorySystem.Pages
{
    [Authorize] // Anyone logged in can access, but we'll redirect borrowers
    public class IndexModel : PageModel
    {
        private readonly InventoryDbContext _context;

        public IndexModel(InventoryDbContext context)
        {
            _context = context;
        }

        public int TotalItems { get; set; }
        public int LowStockCount { get; set; }
        public int CheckedOutCount { get; set; }
        public int OverdueCount { get; set; }
        public List<Item> LowStockItems { get; set; } = new();
        public List<Transaction> RecentTransactions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // REDIRECT BORROWERS TO THEIR OWN PAGE
            if (User.IsInRole("Borrower") && !User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                return RedirectToPage("/Borrower/Index");
            }

            // CHECK IF USER HAS ADMIN OR STAFF ROLE
            if (!User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                // User is logged in but has no role assigned
                // Redirect to borrower page as default
                return RedirectToPage("/Borrower/Index");
            }

            // Load dashboard data for Admin/Staff
            TotalItems = await _context.Items.CountAsync();

            LowStockItems = await _context.Items
                .Where(i => i.CurrentStock <= i.MinStockLevel)
                .ToListAsync();

            LowStockCount = LowStockItems.Count;

            var activeTransactions = await _context.Transactions
                .Where(t => t.Status == TransactionStatus.CheckedOut)
                .ToListAsync();

            CheckedOutCount = activeTransactions.Sum(t => t.Quantity);
            OverdueCount = activeTransactions.Count(t => t.IsOverdue);

            RecentTransactions = await _context.Transactions
                .Include(t => t.Item)
                .Include(t => t.Borrower)
                .Include(t => t.User)
                .OrderByDescending(t => t.TransactionDate)
                .Take(5)
                .ToListAsync();

            return Page();
        }
    }
}
