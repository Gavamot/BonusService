using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BonusService.Migrations
{
    /// <inheritdoc />
    public partial class TransactionHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Transactions");

            migrationBuilder.CreateTable(
                name: "TransactionHistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LastUpdated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    BankId = table.Column<int>(type: "integer", nullable: false),
                    BonusBase = table.Column<long>(type: "bigint", nullable: true),
                    BonusSum = table.Column<long>(type: "bigint", nullable: false),
                    BonusProgramId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false),
                    EzsId = table.Column<Guid>(type: "uuid", nullable: true),
                    OwnerId = table.Column<int>(type: "integer", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    TransactionId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionHistory", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionHistory");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Transactions",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
