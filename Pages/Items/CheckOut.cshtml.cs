using System;
using System.Collections.Generic;
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

namespace CDM.InventorySystem.Pages.Items
{
    [Authorize(Roles = "Admin,Staff")]
    public class CheckOutModel : PageModel
    {
        private readonly ITransactionService _transactionService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly InventoryDbContext _context;

        public CheckOutModel(
            ITransactionService transactionService,
            UserManager<ApplicationUser> userManager,
            InventoryDbContext context)
        {
            _transactionService = transactionService;
            _userManager = userManager;
            _context = context;
        }

        [BindProperty]
        public string ItemId { get; set; } = string.Empty;

        [BindProperty]
        public string BorrowerId { get; set; } = string.Empty;

        [BindProperty]
        public int Quantity { get; set; }

        [BindProperty]
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(14);

        [BindProperty]
        public string Notes { get; set; } = string.Empty;

        public SelectList Users { get; set; } = new SelectList(new List<ApplicationUser>(), "Id", "UserName");

        public async Task OnGetAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            Users = new SelectList(users, "Id", "UserName");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
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
                await _transactionService.CheckOutItemAsync(
                    int.Parse(ItemId), 
                    BorrowerId, 
                    Notes ?? "", 
                    Quantity, 
                    DueDate, 
                    currentUser.Id);

                return RedirectToPage("/Inventory/Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await OnGetAsync();
                return Page();
            }
        }
    }
}