using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CDM.InventorySystem.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; } // Primary key
        public int ItemId { get; set; } 
        public string BorrowerId { get; set; }
        public string UserId { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public TransactionStatus Status { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("ItemId")]
        public virtual Item Item { get; set; }
        
        [ForeignKey("BorrowerId")]
        public virtual ApplicationUser Borrower { get; set; }
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [NotMapped]
        public bool IsOverdue => Status == TransactionStatus.CheckedOut && DueDate < DateTime.Now;
    }

    public enum TransactionStatus
    {
        CheckedOut,
        Returned,
        Overdue
    }
}