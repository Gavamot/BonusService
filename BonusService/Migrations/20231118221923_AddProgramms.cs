using System;
using System.Collections.Generic;
using BonusService.Postgres;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BonusService.Migrations
{
    /// <inheritdoc />
    public partial class AddProgramms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "Transactions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BonusPrograms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ProgramTypes = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    DateStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DateStop = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    BankId = table.Column<List<int>>(type: "integer[]", nullable: false),
                    ExecutionCron = table.Column<string>(type: "text", nullable: false),
                    FrequencyType = table.Column<int>(type: "integer", nullable: false),
                    FrequencyValue = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    LastUpdated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonusPrograms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BonusProgramHistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BonusProgramId = table.Column<int>(type: "integer", nullable: false),
                    ExecTimeStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExecTimeEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Info = table.Column<BonusProgramHistoryInfo>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonusProgramHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BonusProgramHistory_BonusPrograms_BonusProgramId",
                        column: x => x.BonusProgramId,
                        principalTable: "BonusPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BonusProgramsLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    LastUpdated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    BonusProgramId = table.Column<int>(type: "integer", nullable: false),
                    Condition = table.Column<long>(type: "bigint", nullable: false),
                    AwardPercent = table.Column<int>(type: "integer", nullable: false),
                    AwardSum = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonusProgramsLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BonusProgramsLevels_BonusPrograms_BonusProgramId",
                        column: x => x.BonusProgramId,
                        principalTable: "BonusPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BonusProgramHistory_BonusProgramId",
                table: "BonusProgramHistory",
                column: "BonusProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_BonusProgramsLevels_BonusProgramId",
                table: "BonusProgramsLevels",
                column: "BonusProgramId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BonusProgramHistory");

            migrationBuilder.DropTable(
                name: "BonusProgramsLevels");

            migrationBuilder.DropTable(
                name: "BonusPrograms");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Transactions");
        }
    }
}
