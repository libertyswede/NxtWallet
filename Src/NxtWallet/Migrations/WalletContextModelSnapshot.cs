using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using NxtWallet.Model;

namespace NxtWallet.Migrations
{
    [DbContext(typeof(WalletContext))]
    partial class WalletContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348");

            modelBuilder.Entity("NxtWallet.Model.Contact", b =>
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
                });

            modelBuilder.Entity("NxtWallet.Model.Setting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 20);

                    b.Property<string>("Value")
                        .HasAnnotation("MaxLength", 255);

                    b.HasKey("Id");
                });

            modelBuilder.Entity("NxtWallet.Model.Transaction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AccountFrom")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 25);

                    b.Property<string>("AccountTo")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 25);

                    b.Property<bool>("IsConfirmed")
                        .HasAnnotation("Relational:DefaultValue", "True")
                        .HasAnnotation("Relational:DefaultValueType", "System.Boolean");

                    b.Property<string>("Message")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<long>("NqtAmount");

                    b.Property<long>("NqtBalance");

                    b.Property<long>("NqtFee");

                    b.Property<long>("NxtId");

                    b.Property<DateTime>("Timestamp");

                    b.HasKey("Id");
                });
        }
    }
}
