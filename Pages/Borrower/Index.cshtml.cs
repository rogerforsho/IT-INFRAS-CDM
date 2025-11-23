using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CDM.InventorySystem.Data;
using CDM.InventorySystem.Models;
using System.Security.Claims;

namespace CDM.InventorySystem.Pages.Borrower
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly InventoryDbContext _context;

        public IndexModel(InventoryDbContext context)
        {
            _context = context;
        }

        // Properties that the .cshtml page will use
        public List<Transaction> MyTransactions { get; set; } = new();
        public List<Transaction> ActiveTransactions { get; set; } = new();
        public List<Transaction> ReturnedTransactions { get; set; } = new();
        public int OverdueCount { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login");
            }

            MyTransactions = await _context.Transactions
                .Include(t => t.Item)
                .Include(t => t.User)
                .Where(t => t.BorrowerId == userId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            ActiveTransactions = MyTransactions
                .Where(t => t.Status == TransactionStatus.CheckedOut)
                .ToList();

            ReturnedTransactions = MyTransactions
                .Where(t => t.Status == TransactionStatus.Returned)
                .Take(10)
                .ToList();

            OverdueCount = ActiveTransactions.Count(t => t.IsOverdue);

            return Page();
        }
    }
}
