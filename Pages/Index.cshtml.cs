using Microsoft.AspNetCore.Mvc.RazorPages;
using CDM.InventorySystem.Services;
using CDM.InventorySystem.Models;

namespace CDM.InventorySystem.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ITransactionService _transactionService;
        private readonly IItemService _itemService;

        public int TotalItems { get; set; }
        public int CheckedOutCount { get; set; }
        public int OverdueCount { get; set; }
        public int LowStockCount { get; set; }
        public List<Item> LowStockItems { get; set; } = new();
        public List<Transaction> RecentTransactions { get; set; } = new();

        public IndexModel(ITransactionService transactionService, IItemService itemService)
        {
            _transactionService = transactionService;
            _itemService = itemService;
        }

        public async Task OnGetAsync()
        {
            var items = await _itemService.GetAllItemsAsync();
            var overdueTransactions = await _transactionService.GetOverdueTransactionsAsync();
            var transactions = await _transactionService.GetTransactionHistoryAsync();

            TotalItems = items.Count();
            CheckedOutCount = items.Count(i => i.CurrentStock < i.TotalStock);
            OverdueCount = overdueTransactions.Count();
            
            LowStockItems = items
                .Where(i => i.CurrentStock <= i.MinStockLevel)
                .OrderBy(i => i.CurrentStock)
                .Take(5)
                .ToList();
            
            LowStockCount = LowStockItems.Count;

            RecentTransactions = transactions
                .OrderByDescending(t => t.TransactionDate)
                .Take(5)
                .ToList();
        }
    }
}