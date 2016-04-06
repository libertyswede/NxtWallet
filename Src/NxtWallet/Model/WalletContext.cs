using Microsoft.Data.Entity;

namespace NxtWallet.Model
{
    public class WalletContext : DbContext
    {
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=Wallet.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            OnContactCreating(modelBuilder);
            OnSettingCreating(modelBuilder);
            OnTransactionCreating(modelBuilder);
        }

        private static void OnContactCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Contact>()
                .Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(50)
                .HasAnnotation("BackingField", "_name");

            modelBuilder.Entity<Contact>()
                .Property(c => c.NxtAddressRs)
                .IsRequired()
                .HasMaxLength(30);
        }

        private static void OnSettingCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Setting>()
                .Property(s => s.Key)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<Setting>()
                .Property(s => s.Value)
                .HasMaxLength(255);
        }

        private static void OnTransactionCreating(ModelBuilder modelBuilder)
        {
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
                .Property(t => t.NqtBalance)
                .IsRequired();

            modelBuilder.Entity<Transaction>()
                .Property(t => t.NqtFee)
                .IsRequired();

            modelBuilder.Entity<Transaction>()
                .Property(t => t.AccountFrom)
                .IsRequired()
                .HasMaxLength(25);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.AccountTo)
                .IsRequired()
                .HasMaxLength(25);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Message)
                .HasMaxLength(255);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.IsConfirmed)
                .HasDefaultValue(true);
        }
    }
}
