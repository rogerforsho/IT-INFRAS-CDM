using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CDM.InventorySystem.Models;
using CDM.InventorySystem.Services;

namespace CDM.InventorySystem.Controllers.Api
{
    [Route("api/items")]
    [ApiController]
    [Authorize]
    public class ItemsController : ControllerBase
    {
        private readonly IItemService _itemService;

        public ItemsController(IItemService itemService)
        {
            _itemService = itemService;
        }

        [HttpGet("barcode/{barcode}")]
        public async Task<ActionResult<Item>> GetItemByBarcode(string barcode)
        {
            // Normalize input in C# (optional if you do it in the service)
            barcode = barcode?.Trim().ToUpper();

            var item = await _itemService.GetItemByBarcodeAsync(barcode ?? "");
            if (item == null)
            {
                return NotFound(new { message = "Item not found" });
            }

            return Ok(item);
        }

        [HttpGet("low-stock")]
        public async Task<ActionResult<List<Item>>> GetLowStockItems()
        {
            var items = await _itemService.GetLowStockItemsAsync();
            return Ok(items);
        }
    }
}
