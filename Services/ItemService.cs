using CDM.InventorySystem.Data;
using CDM.InventorySystem.Models;
using CDM.InventorySystem.Pages.Items;
using Microsoft.EntityFrameworkCore;

namespace CDM.InventorySystem.Services
{
    public class ItemService : IItemService
    {
        private readonly InventoryDbContext _context;

        public ItemService(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<List<Item>> GetAllItemsAsync()
        {
            return await _context.Items
                .OrderBy(i => i.ItemName)
                .ToListAsync();
        }

        public async Task<Item?> GetItemByIdAsync(int id)
        {
            return await _context.Items.FindAsync(id);
        }

        public async Task<Item?> GetItemByBarcodeAsync(string barcodeId)
        {
            return await _context.Items
                .FirstOrDefaultAsync(i => i.BarcodeId == barcodeId);
        }

        public async Task<bool> AddItemAsync(Item item)
        {
            try
            {
                _context.Items.Add(item);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateItemAsync(Item item)
        {
            try
            {
                _context.Items.Update(item);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteItemAsync(int id)
        {
            try
            {
                var item = await _context.Items.FindAsync(id);
                if (item != null)
                {
                    _context.Items.Remove(item);
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Item>> GetLowStockItemsAsync()
        {
            return await _context.Items
                .Where(i => i.CurrentStock <= i.MinStockLevel)
                .OrderBy(i => i.CurrentStock)
                .ToListAsync();
        }

        public async Task<string> GenerateBarcodeIdAsync(string category)
        {
            var prefix = category switch
            {
                "Keyboard" => "KB",
                "Mouse" => "MS",
                "Monitor" => "MN",
                "Laptop" => "LP",
                "Computer" => "CP",
                "Printer" => "PR",
                "Projector" => "PJ",
                "Tablet" => "TB",
                "Phone" => "PH",
                "Network" => "NW",
                "Accessory" => "AC",
                "Cable" => "CB",
                _ => "IT"
            };

            var lastItem = await _context.Items
                .Where(i => i.BarcodeId.StartsWith($"CDM-{prefix}-"))
                .OrderByDescending(i => i.BarcodeId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastItem != null)
            {
                var parts = lastItem.BarcodeId.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"CDM-{prefix}-{nextNumber:D3}";
        }

        // 🔹 NEW: Get active transaction by barcode
        public async Task<CheckInModel.TransactionViewModel?> GetActiveTransactionByBarcodeAsync(string barcode)
        {
            var activeTransaction = await _context.Transactions
                .Include(t => t.Item)
                .Include(t => t.Borrower)
                .Where(t => t.Item.BarcodeId == barcode && t.ReturnDate == null)
                .OrderBy(t => t.DueDate)
                .FirstOrDefaultAsync();

            if (activeTransaction == null)
                return null;

            return new CheckInModel.TransactionViewModel
            {
                TransactionId = activeTransaction.Id,
                Item = activeTransaction.Item!,
                Borrower = activeTransaction.Borrower!,
                TransactionDate = activeTransaction.TransactionDate,
                DueDate = activeTransaction.DueDate,
                Quantity = activeTransaction.Quantity,
                IsOverdue = activeTransaction.IsOverdue
            };
        }
    }
}
