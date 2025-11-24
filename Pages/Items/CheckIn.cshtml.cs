using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CDM.InventorySystem.Models;
using CDM.InventorySystem.Services;
using CDM.InventorySystem.Data;
using System.ComponentModel.DataAnnotations;

namespace CDM.InventorySystem.Pages.Items
{
    [Authorize(Roles = "Admin,Staff")]
    public class CheckInModel : PageModel
    {
        private readonly ITransactionService _transactionService;
        private readonly InventoryDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CheckInModel> _logger;

        public List<TransactionViewModel> ActiveTransactions { get; set; } = new();
        public List<TransactionViewModel> MatchingTransactions { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        [BindProperty]
        public string Barcode { get; set; } = string.Empty;

        [BindProperty]
        public int? SelectedTransactionId { get; set; }

        public CheckInModel(
            ITransactionService transactionService,
            InventoryDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<CheckInModel> logger)
        {
            _transactionService = transactionService;
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadActiveTransactions();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            try
            {
                var result = await _transactionService.CheckInItemAsync(id, currentUser.Id);
                if (result)
                {
                    StatusMessage = "✅ Item checked in successfully!";
                }
                else
                {
                    StatusMessage = "❌ Failed to check in item. Please try again.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking in item {Id}", id);
                StatusMessage = "❌ An error occurred while checking in the item.";
            }

            return RedirectToPage();
        }

        // Handler for barcode search
        public async Task<IActionResult> OnPostByBarcodeAsync()
        {
            if (string.IsNullOrWhiteSpace(Barcode))
            {
                StatusMessage = "⚠️ Please enter a barcode.";
                await LoadActiveTransactions();
                return Page();
            }

            // Find ALL active transactions for this barcode
            MatchingTransactions = await _context.Transactions
                .Include(t => t.Item)
                .Include(t => t.Borrower)
                .Where(t => t.Item.BarcodeId == Barcode && t.ReturnDate == null)
                .OrderBy(t => t.DueDate)
                .Select(t => new TransactionViewModel
                {
                    TransactionId = t.Id,
                    Item = t.Item!,
                    Borrower = t.Borrower!,
                    TransactionDate = t.TransactionDate,
                    DueDate = t.DueDate,
                    Quantity = t.Quantity,
                    IsOverdue = t.IsOverdue
                })
                .ToListAsync();

            if (!MatchingTransactions.Any())
            {
                StatusMessage = $"⚠️ No active checkout found for barcode '{Barcode}'.";
                await LoadActiveTransactions();
                return Page();
            }

            // If only ONE transaction, check it in immediately
            if (MatchingTransactions.Count == 1)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    var result = await _transactionService.CheckInItemAsync(
                        MatchingTransactions[0].TransactionId,
                        currentUser.Id);

                    StatusMessage = result
                        ? $"✅ Successfully checked in {MatchingTransactions[0].Item.ItemName} from {MatchingTransactions[0].Borrower.FullName}!"
                        : "❌ Failed to check in item.";

                    return RedirectToPage();
                }
            }

            // If MULTIPLE transactions, show selection UI
            StatusMessage = $"⚠️ Found {MatchingTransactions.Count} active checkouts for barcode '{Barcode}'. Please select which one to check in:";
            await LoadActiveTransactions();
            return Page();
        }

        // Handler for confirming selection when multiple matches
        public async Task<IActionResult> OnPostConfirmSelectionAsync()
        {
            if (!SelectedTransactionId.HasValue)
            {
                StatusMessage = "⚠️ Please select a transaction to check in.";
                return RedirectToPage();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            try
            {
                var transaction = await _context.Transactions
                    .Include(t => t.Item)
                    .Include(t => t.Borrower)
                    .FirstOrDefaultAsync(t => t.Id == SelectedTransactionId.Value);

                if (transaction != null)
                {
                    var result = await _transactionService.CheckInItemAsync(
                        SelectedTransactionId.Value,
                        currentUser.Id);

                    StatusMessage = result
                        ? $"✅ Successfully checked in {transaction.Item?.ItemName} from {transaction.Borrower?.FullName}!"
                        : "❌ Failed to check in item.";
                }
                else
                {
                    StatusMessage = "❌ Transaction not found.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking in transaction {Id}", SelectedTransactionId);
                StatusMessage = "❌ An error occurred while checking in the item.";
            }

            return RedirectToPage();
        }

        private async Task LoadActiveTransactions()
        {
            var transactions = await _transactionService.GetTransactionHistoryAsync();
            ActiveTransactions = transactions
                .Where(t => t.ReturnDate == null)
                .OrderBy(t => t.DueDate)
                .Select(t => new TransactionViewModel
                {
                    TransactionId = t.Id,
                    Item = t.Item!,
                    Borrower = t.Borrower!,
                    TransactionDate = t.TransactionDate,
                    DueDate = t.DueDate,
                    Quantity = t.Quantity,
                    IsOverdue = t.IsOverdue
                })
                .ToList();
        }

        public class TransactionViewModel
        {
            public int TransactionId { get; set; }
            public Item Item { get; set; } = null!;
            public ApplicationUser Borrower { get; set; } = null!;
            public DateTime TransactionDate { get; set; }
            public DateTime DueDate { get; set; }
            public int Quantity { get; set; }
            public bool IsOverdue { get; set; }
        }
    }
}
