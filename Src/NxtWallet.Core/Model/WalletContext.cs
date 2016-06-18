using Microsoft.Data.Entity;
using NxtWallet.Core.Migrations.Model;

namespace NxtWallet.Core.Model
{
    public class WalletContext : DbContext
    {
        public DbSet<AssetDto> Assets { get; set; }
        public DbSet<AssetOwnershipDto> AssetOwnerships { get; set; }
        public DbSet<ContactDto> Contacts { get; set; }
        public DbSet<SettingDto> Settings { get; set; }
        public DbSet<TransactionDto> Transactions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=Wallet.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            OnAssetCreating(modelBuilder);
            OnAssetOwnershipCreating(modelBuilder);
            OnContactCreating(modelBuilder);
            OnSettingCreating(modelBuilder);
            OnTransactionCreating(modelBuilder);
        }

        private static void OnAssetCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AssetDto>()
                .Property(a => a.NxtId)
                .IsRequired();

            modelBuilder.Entity<AssetDto>()
                .Property(a => a.Decimals)
                .IsRequired();

            modelBuilder.Entity<AssetDto>()
                .Property(a => a.Account)
                .HasMaxLength(25)
                .IsRequired();

            modelBuilder.Entity<AssetDto>()
                .Property(a => a.Name)
                .HasMaxLength(10)
                .IsRequired();
        }

        private static void OnAssetOwnershipCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AssetOwnershipDto>()
                .Property(o => o.QuantityQnt)
                .IsRequired();

            modelBuilder.Entity<AssetOwnershipDto>()
                .Property(o => o.AssetDecimals)
                .IsRequired();

            modelBuilder.Entity<AssetOwnershipDto>()
                .Property(o => o.BalanceQnt)
                .IsRequired();

            modelBuilder.Entity<AssetOwnershipDto>()
                .Property(o => o.Height)
                .IsRequired();

            modelBuilder.Entity<AssetOwnershipDto>()
                .HasOne(o => o.Transaction)
                .WithOne();
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
                .HasMaxLength(25);

            modelBuilder.Entity<TransactionDto>()
                .Property(t => t.Message)
                .HasMaxLength(255);

            modelBuilder.Entity<TransactionDto>()
                .Property(t => t.IsConfirmed)
                .HasDefaultValue(true);

            modelBuilder.Entity<TransactionDto>()
                .Property(t => t.TransactionType)
                .IsRequired();
        }
    }
}
