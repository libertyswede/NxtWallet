using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using NxtWallet.Model;

namespace NxtWallet.Migrations
{
    [DbContext(typeof(WalletContext))]
    [Migration("20160320164743_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348");

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

                    b.Property<string>("Message")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<long>("NqtAmount");

                    b.Property<long>("NqtFeeAmount");

                    b.Property<long>("NxtId");

                    b.Property<DateTime>("Timestamp");

                    b.HasKey("Id");
                });
        }
    }
}