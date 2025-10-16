namespace CDM.InventorySystem.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Navigation property
        public ApplicationUser? User { get; set; }
    }
}