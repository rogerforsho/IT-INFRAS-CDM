using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CDM.InventorySystem.Data;
using CDM.InventorySystem.Models;

namespace CDM.InventorySystem.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly InventoryDbContext _context;

        public TransactionService(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<Transaction> CheckOutItemAsync(int itemId, string borrowerId, string notes, int quantity, DateTime dueDate, string processorId)
        {
            var item = await _context.Items.FindAsync(itemId);
            if (item == null || item.CurrentStock < quantity)
            {
                throw new InvalidOperationException("Item not found or insufficient stock");
            }

            var transaction = new Transaction
            {
                ItemId = itemId,
                BorrowerId = borrowerId,
                UserId = processorId,
                TransactionDate = DateTime.Now,
                DueDate = dueDate,
                Status = TransactionStatus.CheckedOut,
                TransactionType = "CheckOut",
                Quantity = quantity,
                Notes = notes ?? string.Empty
            };

            // Update item stock
            item.CurrentStock -= quantity;

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return transaction;
        }

        public async Task<bool> CheckInItemAsync(int transactionId, string processorId)
        {
            try
            {
                var transaction = await _context.Transactions
                    .Include(t => t.Item)
                    .FirstOrDefaultAsync(t => t.Id == transactionId && t.ReturnDate == null);

                if (transaction == null || transaction.Item == null)
                    return false;

                // Update transaction
                transaction.ReturnDate = DateTime.Now;
                transaction.UserId = processorId;
                transaction.Status = TransactionStatus.Returned;

                // Restore stock
                transaction.Item.CurrentStock += transaction.Quantity;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Transaction>> GetTransactionHistoryAsync()
        {
            return await _context.Transactions
                .Include(t => t.Item)
                .Include(t => t.Borrower)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<List<Transaction>> GetUserBorrowedItemsAsync(string userId)
        {
            return await _context.Transactions
                .Include(t => t.Item)
                .Where(t => t.BorrowerId == userId && t.ReturnDate == null)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetOverdueTransactionsAsync()
        {
            var today = DateTime.Now;
            return await _context.Transactions
                .Include(t => t.Item)
                .Include(t => t.Borrower)
                .Include(t => t.User)
                .Where(t => t.ReturnDate == null && t.DueDate < today)
                .ToListAsync();
        }
    }
}
