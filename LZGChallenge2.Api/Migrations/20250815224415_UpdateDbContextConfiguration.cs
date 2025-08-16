using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LZGChallenge2.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDbContextConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SummonerId",
                table: "Players",
                type: "nvarchar(78)",
                maxLength: 78,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(56)",
                oldMaxLength: 56);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SummonerId",
                table: "Players",
                type: "nvarchar(56)",
                maxLength: 56,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(78)",
                oldMaxLength: 78);
        }
    }
}
