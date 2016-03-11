using Microsoft.Data.Entity;

namespace NxtWallet.Model
{
    public class WalletContext : DbContext
    {
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=Wallet.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Setting>()
                .Property(s => s.Key)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<Setting>()
                .Property(s => s.Value)
                .HasMaxLength(255);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.NxtId)
                .IsRequired();

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Timestamp)
                .IsRequired();

            modelBuilder.Entity<Transaction>()
                .Property(t => t.NqtAmount)
                .IsRequired();

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Account)
                .IsRequired()
                .HasMaxLength(25);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Message)
                .HasMaxLength(255);
        }
    }
}
