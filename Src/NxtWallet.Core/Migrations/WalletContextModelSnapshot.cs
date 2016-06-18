using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using NxtWallet.Core.Model;

namespace NxtWallet.Core.Migrations
{
    [DbContext(typeof(WalletContext))]
    partial class WalletContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348");

            modelBuilder.Entity("NxtWallet.Migrations.Model.AssetDto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Account")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 25);

                    b.Property<int>("Decimals");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 10);

                    b.Property<long>("NxtId");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "Asset");
                });

            modelBuilder.Entity("NxtWallet.Migrations.Model.AssetOwnershipDto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AssetDecimals");

                    b.Property<int>("AssetId");

                    b.Property<long>("BalanceQnt");

                    b.Property<int>("Height");

                    b.Property<long>("QuantityQnt");

                    b.Property<int>("TransactionId");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "AssetOwnership");
                });

            modelBuilder.Entity("NxtWallet.Migrations.Model.ContactDto", b =>
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

                    b.HasAnnotation("Relational:TableName", "Contact");
                });

            modelBuilder.Entity("NxtWallet.Migrations.Model.SettingDto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 20);

                    b.Property<string>("Value")
                        .HasAnnotation("MaxLength", 255);

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "Setting");
                });

            modelBuilder.Entity("NxtWallet.Migrations.Model.TransactionDto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AccountFrom")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 25);

                    b.Property<string>("AccountTo")
                        .HasAnnotation("MaxLength", 25);

                    b.Property<string>("Extra");

                    b.Property<int>("Height");

                    b.Property<bool>("IsConfirmed")
                        .HasAnnotation("Relational:DefaultValue", "True")
                        .HasAnnotation("Relational:DefaultValueType", "System.Boolean");

                    b.Property<string>("Message")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<long>("NqtAmount");

                    b.Property<long>("NqtBalance");

                    b.Property<long>("NqtFee");

                    b.Property<long?>("NxtId");

                    b.Property<DateTime>("Timestamp");

                    b.Property<int>("TransactionType");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "Transaction");
                });

            modelBuilder.Entity("NxtWallet.Migrations.Model.AssetOwnershipDto", b =>
                {
                    b.HasOne("NxtWallet.Migrations.Model.AssetDto")
                        .WithMany()
                        .HasForeignKey("AssetId");

                    b.HasOne("NxtWallet.Migrations.Model.TransactionDto")
                        .WithOne()
                        .HasForeignKey("NxtWallet.Migrations.Model.AssetOwnershipDto", "TransactionId");
                });
        }
    }
}
