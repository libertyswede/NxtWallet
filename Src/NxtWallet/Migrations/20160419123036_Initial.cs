using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Migrations;

namespace NxtWallet.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetDto",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Account = table.Column<string>(nullable: false),
                    Decimals = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    NxtId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetDto", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "Contact",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false),
                    NxtAddressRs = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactDto", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "Setting",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettingDto", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "TransactionDto",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountFrom = table.Column<string>(nullable: false),
                    AccountTo = table.Column<string>(nullable: true),
                    Extra = table.Column<string>(nullable: true),
                    Height = table.Column<int>(nullable: false),
                    IsConfirmed = table.Column<bool>(nullable: false, defaultValue: true),
                    Message = table.Column<string>(nullable: true),
                    NqtAmount = table.Column<long>(nullable: false),
                    NqtBalance = table.Column<long>(nullable: false),
                    NqtFee = table.Column<long>(nullable: false),
                    NxtId = table.Column<long>(nullable: true),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    TransactionType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionDto", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "AssetOwnershipDto",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AssetDecimals = table.Column<int>(nullable: false),
                    AssetId = table.Column<int>(nullable: false),
                    BalanceQnt = table.Column<long>(nullable: false),
                    Height = table.Column<int>(nullable: false),
                    QuantityQnt = table.Column<long>(nullable: false),
                    TransactionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetOwnershipDto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetOwnershipDto_AssetDto_AssetId",
                        column: x => x.AssetId,
                        principalTable: "AssetDto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssetOwnershipDto_TransactionDto_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "TransactionDto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("AssetOwnershipDto");
            migrationBuilder.DropTable("Contact");
            migrationBuilder.DropTable("Setting");
            migrationBuilder.DropTable("AssetDto");
            migrationBuilder.DropTable("TransactionDto");
        }
    }
}
