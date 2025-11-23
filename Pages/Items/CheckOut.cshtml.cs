using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using CDM.InventorySystem.Services;
using CDM.InventorySystem.Data;
using Microsoft.EntityFrameworkCore;
using CDM.InventorySystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace CDM.InventorySystem.Pages.Items
{
    [Authorize(Roles = "Admin,Staff")]
    public class CheckOutModel : PageModel
    {
        private readonly ITransactionService _transactionService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly InventoryDbContext _context;
        private readonly ILogger<CheckOutModel> _logger;

        public CheckOutModel(
            ITransactionService transactionService,
            UserManager<ApplicationUser> userManager,
            InventoryDbContext context,
            ILogger<CheckOutModel> logger)
        {
            _transactionService = transactionService;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public string ItemId { get; set; } = string.Empty;

        [BindProperty]
        public string BorrowerId { get; set; } = string.Empty;

        [BindProperty]
        public int Quantity { get; set; } = 1;

        [BindProperty]
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(14);

        [BindProperty]
        public string Notes { get; set; } = string.Empty;

        public SelectList Users { get; set; } = new SelectList(new List<ApplicationUser>(), "Id", "FullName");
        public SelectList Items { get; set; } = new SelectList(new List<Item>(), "ItemId", "ItemName");

        [TempData]
        public string? StatusMessage { get; set; }

        // Debug/Full version of OnGetAsync
        public async Task OnGetAsync()
        {
            try
            {
                // Test database connection
                var userCount = await _userManager.Users.CountAsync();
                _logger.LogInformation($"Database connected. User count: {userCount}");

                // Load users
                var users = await _userManager.Users.ToListAsync();
                _logger.LogInformation($"Loaded {users.Count} users");

                // Create SelectList (FullName fallback to Email)
                if (users.Any())
                {
                    Users = new SelectList(
                        users.Select(u => new
                        {
                            u.Id,
                            DisplayName = !string.IsNullOrEmpty(u.FullName) ? u.FullName : u.Email
                        }),
                        "Id",
                        "DisplayName"
                    );
                    _logger.LogInformation($"SelectList created with {Users.Count()} items");
                }
                else
                {
                    _logger.LogWarning("No users found. SelectList is empty.");
                }

                // Load items
                var items = await _context.Items.ToListAsync();
                Items = new SelectList(items, "ItemId", "ItemName");
                _logger.LogInformation($"Loaded {items.Count} items");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnGetAsync");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation($"DEBUG - ItemId: {ItemId}, BorrowerId: {BorrowerId}, Quantity: {Quantity}");

            if (string.IsNullOrEmpty(ItemId))
            {
                ModelState.AddModelError("", "Please select an item");
                await OnGetAsync();
                return Page();
            }

            if (string.IsNullOrEmpty(BorrowerId))
            {
                ModelState.AddModelError("", "Please select a borrower");
                await OnGetAsync();
                return Page();
            }

            if (Quantity <= 0)
            {
                ModelState.AddModelError("", "Quantity must be greater than 0");
                await OnGetAsync();
                return Page();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            try
            {
                if (!int.TryParse(ItemId, out int itemIdInt))
                {
                    ModelState.AddModelError("", $"Invalid Item ID: {ItemId}");
                    await OnGetAsync();
                    return Page();
                }

                var item = await _context.Items.FindAsync(itemIdInt);
                if (item == null)
                {
                    ModelState.AddModelError("", $"Item with ID {itemIdInt} not found");
                    await OnGetAsync();
                    return Page();
                }

                if (item.CurrentStock < Quantity)
                {
                    ModelState.AddModelError("",
                        $"Insufficient stock for {item.ItemName}. Available: {item.CurrentStock}, Requested: {Quantity}");
                    await OnGetAsync();
                    return Page();
                }

                var borrower = await _userManager.FindByIdAsync(BorrowerId);
                if (borrower == null)
                {
                    ModelState.AddModelError("", "Selected borrower not found");
                    await OnGetAsync();
                    return Page();
                }

                var transaction = await _transactionService.CheckOutItemAsync(
                    itemIdInt,
                    BorrowerId,
                    Notes ?? "",
                    Quantity,
                    DueDate,
                    currentUser.Id);

                TempData["SuccessMessage"] =
                    $"Successfully checked out {Quantity} {item.ItemName}(s) to {borrower.FullName ?? borrower.Email}";

                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                _logger.LogError(ex, "Error during OnPostAsync checkout");
                await OnGetAsync();
                return Page();
            }
        }
    }
}
