using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CDM.InventorySystem.Models;

namespace CDM.InventorySystem.Services
{
    public interface ITransactionService
    {
        Task<Transaction> CheckOutItemAsync(int itemId, string borrowerId, string notes, int quantity, DateTime dueDate, string processorId);
        Task<bool> CheckInItemAsync(int transactionId, string processorId);
        Task<List<Transaction>> GetTransactionHistoryAsync();
        Task<List<Transaction>> GetUserBorrowedItemsAsync(string userId);
        Task<IEnumerable<Transaction>> GetOverdueTransactionsAsync();
    }
}
