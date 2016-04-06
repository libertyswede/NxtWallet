using Microsoft.Data.Entity;

namespace NxtWallet.Model
{
    public class WalletContext : DbContext
    {
        public DbSet<ContactDto> Contacts { get; set; }
        public DbSet<SettingDto> Settings { get; set; }
        public DbSet<TransactionDto> Transactions { get; set; }

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
            modelBuilder.Entity<ContactDto>()
                .Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<ContactDto>()
                .Property(c => c.NxtAddressRs)
                .IsRequired()
                .HasMaxLength(30);
        }

        private static void OnSettingCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SettingDto>()
                .Property(s => s.Key)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<SettingDto>()
                .Property(s => s.Value)
                .HasMaxLength(255);
        }

        private static void OnTransactionCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TransactionDto>()
                .Property(t => t.NxtId)
                .IsRequired();

            modelBuilder.Entity<TransactionDto>()
                .Property(t => t.Timestamp)
                .IsRequired();

            modelBuilder.Entity<TransactionDto>()
                .Property(t => t.NqtAmount)
                .IsRequired();

            modelBuilder.Entity<TransactionDto>()
                .Property(t => t.NqtBalance)
                .IsRequired();

            modelBuilder.Entity<TransactionDto>()
                .Property(t => t.NqtFee)
                .IsRequired();

            modelBuilder.Entity<TransactionDto>()
                .Property(t => t.AccountFrom)
                .IsRequired()
                .HasMaxLength(25);

            modelBuilder.Entity<TransactionDto>()
                .Property(t => t.AccountTo)
                .IsRequired()
                .HasMaxLength(25);

            modelBuilder.Entity<TransactionDto>()
                .Property(t => t.Message)
                .HasMaxLength(255);

            modelBuilder.Entity<TransactionDto>()
                .Property(t => t.IsConfirmed)
                .HasDefaultValue(true);
        }
    }
}
