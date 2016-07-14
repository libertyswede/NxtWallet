using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using NxtWallet.Repositories.Model;

namespace NxtWallet.Core.Migrations
{
    [DbContext(typeof(WalletContext))]
    partial class WalletContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348");

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

                    b.HasAnnotation("Relational:TableName", "Contact");
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

                    b.Property<int>("Height");

                    b.Property<bool>("IsConfirmed")
                        .HasAnnotation("Relational:DefaultValue", "True")
                        .HasAnnotation("Relational:DefaultValueType", "System.Boolean");

                    b.Property<string>("Message");

                    b.Property<long>("NqtAmount");

                    b.Property<long>("NqtBalance");

                    b.Property<long>("NqtFee");

                    b.Property<DateTime>("Timestamp");

                    b.Property<long?>("TransactionId");

                    b.Property<int>("TransactionType");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "LedgerEntry");
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

                    b.HasAnnotation("Relational:TableName", "Setting");
                });
        }
    }
}
