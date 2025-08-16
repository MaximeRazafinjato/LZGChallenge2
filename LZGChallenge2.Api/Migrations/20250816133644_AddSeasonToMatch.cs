using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LZGChallenge2.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSeasonToMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Season",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Season",
                table: "Matches");
        }
    }
}
