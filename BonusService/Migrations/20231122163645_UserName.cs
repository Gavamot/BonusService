using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BonusService.Migrations
{
    /// <inheritdoc />
    public partial class UserName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TransactionHistory");

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Transactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "TransactionHistory",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "TransactionHistory");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "TransactionHistory",
                type: "uuid",
                nullable: true);
        }
    }
}
