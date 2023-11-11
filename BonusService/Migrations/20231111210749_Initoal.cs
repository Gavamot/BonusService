using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BonusService.Migrations
{
    /// <inheritdoc />
    public partial class Initoal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LastUpdated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    BankId = table.Column<int>(type: "integer", nullable: false),
                    BonusBase = table.Column<long>(type: "bigint", nullable: true),
                    BonusSum = table.Column<long>(type: "bigint", nullable: false),
                    BonusProgramId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false),
                    EzsId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    TransactionId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_PersonId_BankId",
                table: "Transactions",
                columns: new[] { "PersonId", "BankId" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransactionId",
                table: "Transactions",
                column: "TransactionId",
                unique: true)
                .Annotation("Npgsql:IndexInclude", new[] { "BonusSum" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions");
        }
    }
}
