using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SongTheSame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountsLeaguesCustomGames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_puzzle_rounds_daily_puzzles_PuzzleId",
                table: "puzzle_rounds");

            migrationBuilder.DropTable(
                name: "daily_puzzles");

            migrationBuilder.DropIndex(
                name: "IX_game_sessions_PlayerKey_Day",
                table: "game_sessions");

            migrationBuilder.CreateTable(
                name: "leagues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    InviteCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leagues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "profiles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_profiles", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "puzzles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Day = table.Column<DateOnly>(type: "date", nullable: false),
                    Code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Title = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_puzzles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "league_members",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeagueId = table.Column<Guid>(type: "uuid", nullable: false),
                    JoinedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_league_members", x => new { x.LeagueId, x.UserId });
                    table.ForeignKey(
                        name: "FK_league_members_leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_game_sessions_PlayerKey_PuzzleId",
                table: "game_sessions",
                columns: new[] { "PlayerKey", "PuzzleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_league_members_UserId",
                table: "league_members",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_leagues_InviteCode",
                table: "leagues",
                column: "InviteCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_puzzles_Code",
                table: "puzzles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_puzzles_Day",
                table: "puzzles",
                column: "Day",
                unique: true,
                filter: "\"Kind\" = 'Daily'");

            migrationBuilder.CreateIndex(
                name: "IX_puzzles_OwnerId",
                table: "puzzles",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_puzzle_rounds_puzzles_PuzzleId",
                table: "puzzle_rounds",
                column: "PuzzleId",
                principalTable: "puzzles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_puzzle_rounds_puzzles_PuzzleId",
                table: "puzzle_rounds");

            migrationBuilder.DropTable(
                name: "league_members");

            migrationBuilder.DropTable(
                name: "profiles");

            migrationBuilder.DropTable(
                name: "puzzles");

            migrationBuilder.DropTable(
                name: "leagues");

            migrationBuilder.DropIndex(
                name: "IX_game_sessions_PlayerKey_PuzzleId",
                table: "game_sessions");

            migrationBuilder.CreateTable(
                name: "daily_puzzles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Day = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_puzzles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_game_sessions_PlayerKey_Day",
                table: "game_sessions",
                columns: new[] { "PlayerKey", "Day" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_daily_puzzles_Day",
                table: "daily_puzzles",
                column: "Day",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_puzzle_rounds_daily_puzzles_PuzzleId",
                table: "puzzle_rounds",
                column: "PuzzleId",
                principalTable: "daily_puzzles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
