using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using NxtWallet.Repositories.Model;

namespace NxtWallet.Core.Migrations
{
    [DbContext(typeof(WalletContext))]
    [Migration("20160814211709_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431");

            modelBuilder.Entity("NxtWallet.Core.Migrations.Model.ContactDto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 50);

                    b.Property<string>("NxtAddressRs")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 30);

                    b.HasKey("Id");

                    b.HasIndex("NxtAddressRs")
                        .IsUnique();

                    b.ToTable("Contact");
                });

            modelBuilder.Entity("NxtWallet.Core.Migrations.Model.LedgerEntryDto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AccountFrom")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 25);

                    b.Property<string>("AccountTo")
                        .HasAnnotation("MaxLength", 25);

                    b.Property<long?>("BlockId");

                    b.Property<DateTime>("BlockTimestamp");

                    b.Property<string>("EncryptedMessage");

                    b.Property<int?>("Height");

                    b.Property<bool>("IsConfirmed")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(true);

                    b.Property<string>("NoteToSelfMessage");

                    b.Property<long>("NqtAmount");

                    b.Property<long>("NqtBalance");

                    b.Property<long>("NqtFee");

                    b.Property<string>("PlainMessage");

                    b.Property<long?>("TransactionId");

                    b.Property<DateTime>("TransactionTimestamp");

                    b.Property<int>("TransactionType");

                    b.HasKey("Id");

                    b.ToTable("LedgerEntry");
                });

            modelBuilder.Entity("NxtWallet.Core.Migrations.Model.SettingDto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 20);

                    b.Property<string>("Value")
                        .HasAnnotation("MaxLength", 255);

                    b.HasKey("Id");

                    b.ToTable("Setting");
                });
        }
    }
}
