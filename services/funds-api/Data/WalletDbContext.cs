using Microsoft.EntityFrameworkCore;
using FundsApi.Models;

namespace FundsApi.Data
{
    public class WalletDbContext : DbContext
    {
        public WalletDbContext(DbContextOptions<WalletDbContext> options) : base(options) { }

        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<TransactionEvent> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // --- Wallet Configuration ---
    modelBuilder.Entity<Wallet>(entity =>
    {
        entity.HasKey(w => w.Id);
        entity.Property(w => w.AvailableFunds)
              .HasPrecision(18, 2);
    });

    // --- TransactionEvent Configuration ---
    modelBuilder.Entity<TransactionEvent>(entity =>
    {
        entity.HasKey(t => t.Id);
        entity.Property(t => t.Amount)
              .HasPrecision(18, 2);
        entity.HasOne(t => t.Wallet)
              .WithMany(w => w.Transactions)
              .HasForeignKey(t => t.WalletId);
    });

    // --- Data Seeding ---
    
    // Seeding for Wallets
    modelBuilder.Entity<Wallet>().HasData(
        new Wallet { 
            Id = 1, 
            AvailableFunds = 1000.00m, 
        },
        new Wallet { 
            Id = 2, 
            AvailableFunds = 500.00m, 
        },
        new Wallet { 
            Id = 3, 
            AvailableFunds = 0.00m, 
        }
    );

    // Seeding for TransactionEvents
    modelBuilder.Entity<TransactionEvent>().HasData(
        new TransactionEvent { 
            Id = 1, 
            WalletId = 1, 
            Amount = 100.00m, 
            Type = "Deposit", 
            Timestamp = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc) 
        },
        new TransactionEvent { 
            Id = 2, 
            WalletId = 1, 
            Amount = -50.00m, 
            Type = "Withdrawal", 
            Timestamp = new DateTime(2026, 1, 1, 11, 0, 0, DateTimeKind.Utc) 
        },
        new TransactionEvent { 
            Id = 3, 
            WalletId = 2, 
            Amount = 200.00m, 
            Type = "Deposit", 
            Timestamp = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc) 
        }
    );
}
    }
}