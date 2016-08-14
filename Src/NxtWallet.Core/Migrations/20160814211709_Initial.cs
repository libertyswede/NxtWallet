using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NxtWallet.Core.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contact",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    NxtAddressRs = table.Column<string>(maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contact", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LedgerEntry",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    AccountFrom = table.Column<string>(maxLength: 25, nullable: false),
                    AccountTo = table.Column<string>(maxLength: 25, nullable: true),
                    BlockId = table.Column<long>(nullable: true),
                    BlockTimestamp = table.Column<DateTime>(nullable: false),
                    EncryptedMessage = table.Column<string>(nullable: true),
                    Height = table.Column<int>(nullable: true),
                    IsConfirmed = table.Column<bool>(nullable: false, defaultValue: true),
                    NoteToSelfMessage = table.Column<string>(nullable: true),
                    NqtAmount = table.Column<long>(nullable: false),
                    NqtBalance = table.Column<long>(nullable: false),
                    NqtFee = table.Column<long>(nullable: false),
                    PlainMessage = table.Column<string>(nullable: true),
                    TransactionId = table.Column<long>(nullable: true),
                    TransactionTimestamp = table.Column<DateTime>(nullable: false),
                    TransactionType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LedgerEntry", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Setting",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    Key = table.Column<string>(maxLength: 20, nullable: false),
                    Value = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Setting", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contact_NxtAddressRs",
                table: "Contact",
                column: "NxtAddressRs",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contact");

            migrationBuilder.DropTable(
                name: "LedgerEntry");

            migrationBuilder.DropTable(
                name: "Setting");
        }
    }
}
