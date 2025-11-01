using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CDM.InventorySystem.Models;
using CDM.InventorySystem.Services;
using System.ComponentModel.DataAnnotations;

namespace CDM.InventorySystem.Pages.Items
{
    [Authorize]
    public class CheckInModel : PageModel
    {
        private readonly ITransactionService _transactionService;
        private readonly IItemService _itemService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CheckInModel> _logger;

        public List<TransactionViewModel> ActiveTransactions { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public CheckInModel(
            ITransactionService transactionService,
            IItemService itemService,
            UserManager<ApplicationUser> userManager,
            ILogger<CheckInModel> logger)
        {
            _transactionService = transactionService;
            _itemService = itemService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
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
                    StatusMessage = "Item checked in successfully.";
                }
                else
                {
                    StatusMessage = "Failed to check in item. Please try again.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking in item {Id}", id);
                StatusMessage = "An error occurred while checking in the item.";
            }

            return RedirectToPage();
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