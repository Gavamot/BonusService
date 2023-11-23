using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BonusService.Migrations
{
    /// <inheritdoc />
    public partial class RenameBonusHistoryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DurationMilliseconds",
                table: "BonusProgramHistory",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationMilliseconds",
                table: "BonusProgramHistory");
        }
    }
}
