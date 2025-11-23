using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using CDM.InventorySystem.Models;
using CDM.InventorySystem.Services;
using Microsoft.EntityFrameworkCore;

namespace CDM.InventorySystem.Pages.Transactions
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ITransactionService _transactionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public List<Transaction> Transactions { get; set; } = new();
        public List<ApplicationUser> Users { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string TransactionType { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string Status { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string BorrowerId { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public DateOnly? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 20;
        public int TotalTransactions { get; set; }
        public int TotalPages { get; set; }
        public int ActiveCheckouts { get; set; }
        public int OverdueCount { get; set; }
        public int ReturnedCount { get; set; }

        public IndexModel(ITransactionService transactionService, UserManager<ApplicationUser> userManager)
        {
            _transactionService = transactionService;
            _userManager = userManager;
        }

        public async Task OnGetAsync()
        {
            await LoadUsersAsync();
            await LoadTransactionsAsync();
            await LoadStatisticsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadTransactionsAsync();
                return Page();
            }
            // Re-run loads with current filters
            await OnGetAsync();
            return Page();
        }

        private async Task LoadUsersAsync()
        {
            Users = await _userManager.Users
                .Where(u => u.Role == "Student" || u.Role == "Staff")
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        private async Task LoadTransactionsAsync()
        {
            var allTransactions = await _transactionService.GetTransactionHistoryAsync();

            var filteredTransactions = allTransactions.AsQueryable();

            if (!string.IsNullOrEmpty(TransactionType))
            {
                filteredTransactions = filteredTransactions.Where(t => t.TransactionType == TransactionType);
            }

            if (!string.IsNullOrEmpty(BorrowerId))
            {
                filteredTransactions = filteredTransactions.Where(t => t.BorrowerId == BorrowerId);
            }

            if (StartDate.HasValue)
            {
                filteredTransactions = filteredTransactions.Where(t =>
                    t.TransactionDate >= StartDate.Value.ToDateTime(TimeOnly.MinValue));
            }

            if (!string.IsNullOrEmpty(Status))
            {
                filteredTransactions = Status switch
                {
                    "Active" => filteredTransactions.Where(t => t.ReturnDate == null && !t.IsOverdue),
                    "Returned" => filteredTransactions.Where(t => t.ReturnDate.HasValue),
                    "Overdue" => filteredTransactions.Where(t => t.IsOverdue && t.ReturnDate == null),
                    _ => filteredTransactions
                };
            }

            TotalTransactions = filteredTransactions.Count();
            TotalPages = (int)Math.Ceiling(TotalTransactions / (double)PageSize);

            Transactions = filteredTransactions
                .OrderByDescending(t => t.TransactionDate)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        private async Task LoadStatisticsAsync()
        {
            var allTransactions = await _transactionService.GetTransactionHistoryAsync();

            ActiveCheckouts = allTransactions.Count(t => t.ReturnDate == null && !t.IsOverdue);
            OverdueCount = allTransactions.Count(t => t.IsOverdue && t.ReturnDate == null);
            ReturnedCount = allTransactions.Count(t => t.ReturnDate.HasValue);
        }
    }
}