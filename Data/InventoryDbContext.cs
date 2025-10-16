using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CDM.InventorySystem.Models;

namespace CDM.InventorySystem.Data
{
    public class InventoryDbContext : IdentityDbContext<ApplicationUser>
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
            : base(options)
        {
        }

        public DbSet<Item> Items { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Item
            modelBuilder.Entity<Item>(entity =>
            {
                entity.HasIndex(i => i.BarcodeId).IsUnique();   
                entity.Property(i => i.PurchasePrice).HasPrecision(10, 2);
            });

            // Configure Transaction
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasOne(t => t.Item)
                    .WithMany(i => i.Transactions)
                    .HasForeignKey(t => t.ItemId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Borrower)
                    .WithMany(u => u.BorrowedTransactions)
                    .HasForeignKey(t => t.BorrowerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.User)
                    .WithMany(u => u.ProcessedTransactions)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure ActivityLog
            modelBuilder.Entity<ActivityLog>(entity =>
            {
                entity.HasOne(a => a.User)
                    .WithMany(u => u.ActivityLogs)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}