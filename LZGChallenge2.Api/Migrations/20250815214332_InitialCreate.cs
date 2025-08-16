using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LZGChallenge2.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RiotId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GameName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TagLine = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Puuid = table.Column<string>(type: "nvarchar(78)", maxLength: 78, nullable: false),
                    SummonerId = table.Column<string>(type: "nvarchar(56)", maxLength: 56, nullable: false),
                    Region = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChampionStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    ChampionId = table.Column<int>(type: "int", nullable: false),
                    ChampionName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GamesPlayed = table.Column<int>(type: "int", nullable: false),
                    Wins = table.Column<int>(type: "int", nullable: false),
                    Losses = table.Column<int>(type: "int", nullable: false),
                    TotalKills = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeaths = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAssists = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AverageCreepScore = table.Column<double>(type: "float", nullable: false),
                    AverageVisionScore = table.Column<double>(type: "float", nullable: false),
                    AverageDamageDealt = table.Column<double>(type: "float", nullable: false),
                    AverageGameDuration = table.Column<double>(type: "float", nullable: false),
                    LastPlayedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChampionStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChampionStats_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatchId = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    GameStartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GameEndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GameDuration = table.Column<long>(type: "bigint", nullable: false),
                    Win = table.Column<bool>(type: "bit", nullable: false),
                    QueueId = table.Column<int>(type: "int", nullable: false),
                    Kills = table.Column<int>(type: "int", nullable: false),
                    Deaths = table.Column<int>(type: "int", nullable: false),
                    Assists = table.Column<int>(type: "int", nullable: false),
                    ChampionId = table.Column<int>(type: "int", nullable: false),
                    ChampionName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Position = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    TotalDamageDealtToChampions = table.Column<long>(type: "bigint", nullable: false),
                    TotalDamageTaken = table.Column<long>(type: "bigint", nullable: false),
                    GoldEarned = table.Column<long>(type: "bigint", nullable: false),
                    CreepScore = table.Column<int>(type: "int", nullable: false),
                    VisionScore = table.Column<int>(type: "int", nullable: false),
                    Tier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rank = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LeaguePoints = table.Column<int>(type: "int", nullable: true),
                    LpChange = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matches_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    CurrentTier = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CurrentRank = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    CurrentLeaguePoints = table.Column<int>(type: "int", nullable: false),
                    TotalGames = table.Column<int>(type: "int", nullable: false),
                    TotalWins = table.Column<int>(type: "int", nullable: false),
                    TotalLosses = table.Column<int>(type: "int", nullable: false),
                    TotalKills = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeaths = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAssists = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AverageCreepScore = table.Column<double>(type: "float", nullable: false),
                    AverageVisionScore = table.Column<double>(type: "float", nullable: false),
                    AverageGameDuration = table.Column<double>(type: "float", nullable: false),
                    AverageDamageDealt = table.Column<double>(type: "float", nullable: false),
                    CurrentWinStreak = table.Column<int>(type: "int", nullable: false),
                    CurrentLoseStreak = table.Column<int>(type: "int", nullable: false),
                    LongestWinStreak = table.Column<int>(type: "int", nullable: false),
                    LongestLoseStreak = table.Column<int>(type: "int", nullable: false),
                    TotalLpGained = table.Column<int>(type: "int", nullable: false),
                    TotalLpLost = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerStats_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GamesPlayed = table.Column<int>(type: "int", nullable: false),
                    Wins = table.Column<int>(type: "int", nullable: false),
                    Losses = table.Column<int>(type: "int", nullable: false),
                    PlayRate = table.Column<double>(type: "float", nullable: false),
                    TotalKills = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeaths = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAssists = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AverageCreepScore = table.Column<double>(type: "float", nullable: false),
                    AverageVisionScore = table.Column<double>(type: "float", nullable: false),
                    AverageDamageDealt = table.Column<double>(type: "float", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleStats_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChampionStats_PlayerId_ChampionId",
                table: "ChampionStats",
                columns: new[] { "PlayerId", "ChampionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_GameStartTime",
                table: "Matches",
                column: "GameStartTime");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_MatchId",
                table: "Matches",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_PlayerId_MatchId",
                table: "Matches",
                columns: new[] { "PlayerId", "MatchId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_GameName_TagLine",
                table: "Players",
                columns: new[] { "GameName", "TagLine" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_Puuid",
                table: "Players",
                column: "Puuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_SummonerId",
                table: "Players",
                column: "SummonerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStats_PlayerId",
                table: "PlayerStats",
                column: "PlayerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleStats_PlayerId_Position",
                table: "RoleStats",
                columns: new[] { "PlayerId", "Position" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChampionStats");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "PlayerStats");

            migrationBuilder.DropTable(
                name: "RoleStats");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
