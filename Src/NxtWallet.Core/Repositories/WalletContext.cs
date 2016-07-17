using System;
using Microsoft.Data.Entity;
using NxtWallet.Core.Migrations.Model;

namespace NxtWallet.Repositories.Model
{
    public class WalletContext : DbContext
    {
        public DbSet<ContactDto> Contacts { get; set; }
        public DbSet<LedgerEntryDto> LedgerEntries { get; set; }
        public DbSet<SettingDto> Settings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=Wallet.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            OnContactCreating(modelBuilder);
            OnLedgerEntryCreating(modelBuilder);
            OnSettingCreating(modelBuilder);
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

        private void OnLedgerEntryCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LedgerEntryDto>()
                .Property(t => t.Timestamp)
                .IsRequired();

            modelBuilder.Entity<LedgerEntryDto>()
                .Property(t => t.NqtBalance)
                .IsRequired();

            modelBuilder.Entity<LedgerEntryDto>()
                .Property(t => t.NqtAmount)
                .IsRequired();

            modelBuilder.Entity<LedgerEntryDto>()
                .Property(t => t.NqtFee)
                .IsRequired();

            modelBuilder.Entity<LedgerEntryDto>()
                .Property(t => t.AccountFrom)
                .IsRequired()
                .HasMaxLength(25);

            modelBuilder.Entity<LedgerEntryDto>()
                .Property(t => t.AccountTo)
                .HasMaxLength(25);

            modelBuilder.Entity<LedgerEntryDto>()
                .Property(t => t.IsConfirmed)
                .HasDefaultValue(true);

            modelBuilder.Entity<LedgerEntryDto>()
                .Property(t => t.TransactionType)
                .IsRequired();
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
    }
}
