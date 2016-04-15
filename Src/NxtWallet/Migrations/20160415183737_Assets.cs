using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Migrations;

namespace NxtWallet.Migrations
{
    public partial class Assets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetDto",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Decimals = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    NxtId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetDto", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "AssetOwnershipDto",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AssetId = table.Column<int>(nullable: false),
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
                        principalTable: "Transaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "Transaction",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Height", table: "Transaction");
            migrationBuilder.DropTable("AssetOwnershipDto");
            migrationBuilder.DropTable("AssetDto");
        }
    }
}
