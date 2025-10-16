using System.ComponentModel.DataAnnotations;

namespace CDM.InventorySystem.Models
{
    public class Item
    {
        public int ItemId { get; set; }

        [Required]
        public string BarcodeId { get; set; } = string.Empty;

        [Required]
        public string ItemName { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Specification { get; set; }

        [Range(0, int.MaxValue)]
        public int CurrentStock { get; set; } = 0;

        [Range(0, int.MaxValue)]
        public int TotalStock { get; set; }

        [Range(1, int.MaxValue)]
        public int MinStockLevel { get; set; } = 5;

        [Required]
        public string Location { get; set; } = string.Empty;
        
        public string Condition { get; set; } = "Good";
        public string? Vendor { get; set; }
        public DateOnly? PurchaseDate { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal PurchasePrice { get; set; }
        
        public DateTime DateAdded { get; set; } = DateTime.Now;

        // Navigation properties
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}