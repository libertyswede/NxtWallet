using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Migrations;

namespace NxtWallet.Migrations
{
    public partial class Transaction_IsConfirmed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsConfirmed",
                table: "Transaction",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IsConfirmed", table: "Transaction");
        }
    }
}
