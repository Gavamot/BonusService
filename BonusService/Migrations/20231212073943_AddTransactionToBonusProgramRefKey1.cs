using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BonusService.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionToBonusProgramRefKey1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Transactions_BonusProgramId",
                table: "Transactions",
                column: "BonusProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionHistory_BonusProgramId",
                table: "TransactionHistory",
                column: "BonusProgramId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionHistory_BonusPrograms_BonusProgramId",
                table: "TransactionHistory",
                column: "BonusProgramId",
                principalTable: "BonusPrograms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_BonusPrograms_BonusProgramId",
                table: "Transactions",
                column: "BonusProgramId",
                principalTable: "BonusPrograms",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionHistory_BonusPrograms_BonusProgramId",
                table: "TransactionHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_BonusPrograms_BonusProgramId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_BonusProgramId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_TransactionHistory_BonusProgramId",
                table: "TransactionHistory");
        }
    }
}
