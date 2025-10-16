using Microsoft.AspNetCore.Identity;

namespace CDM.InventorySystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "Student";
        public string? Department { get; set; }
        public DateTime DateRegistered { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<Transaction> ProcessedTransactions { get; set; } = new List<Transaction>();
        public virtual ICollection<Transaction> BorrowedTransactions { get; set; } = new List<Transaction>();
        public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    }
}