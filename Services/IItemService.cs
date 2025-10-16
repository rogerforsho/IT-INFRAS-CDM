using CDM.InventorySystem.Models;

namespace CDM.InventorySystem.Services
{
    public interface IItemService
    {
        Task<List<Item>> GetAllItemsAsync();
        Task<Item?> GetItemByIdAsync(int id);
        Task<Item?> GetItemByBarcodeAsync(string barcodeId);
        Task<bool> AddItemAsync(Item item);
        Task<bool> UpdateItemAsync(Item item);
        Task<bool> DeleteItemAsync(int id);
        Task<List<Item>> GetLowStockItemsAsync();
        Task<string> GenerateBarcodeIdAsync(string category);
    }
}