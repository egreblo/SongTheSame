using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SongTheSame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGameplay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "daily_puzzles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Day = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_puzzles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "game_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    PuzzleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Day = table.Column<DateOnly>(type: "date", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_sessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "puzzle_rounds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Index = table.Column<int>(type: "integer", nullable: false),
                    SongId = table.Column<Guid>(type: "uuid", nullable: false),
                    SegmentStartFrame = table.Column<int>(type: "integer", nullable: false),
                    SegmentStartSeconds = table.Column<double>(type: "double precision", nullable: false),
                    SegmentSeconds = table.Column<double>(type: "double precision", nullable: false),
                    PuzzleId = table.Column<Guid>(type: "uuid", nullable: true),
                    BestMatches = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_puzzle_rounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_puzzle_rounds_daily_puzzles_PuzzleId",
                        column: x => x.PuzzleId,
                        principalTable: "daily_puzzles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "round_progress",
                columns: table => new
                {
                    Index = table.Column<int>(type: "integer", nullable: false),
                    GameSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Phase1Attempts = table.Column<int>(type: "integer", nullable: false),
                    Phase1Solved = table.Column<bool>(type: "boolean", nullable: false),
                    Phase1Done = table.Column<bool>(type: "boolean", nullable: false),
                    Phase1Guesses = table.Column<List<Guid>>(type: "uuid[]", nullable: false),
                    Phase2SongId = table.Column<Guid>(type: "uuid", nullable: true),
                    Phase2Score = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_round_progress", x => new { x.GameSessionId, x.Index });
                    table.ForeignKey(
                        name: "FK_round_progress_game_sessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "game_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_daily_puzzles_Day",
                table: "daily_puzzles",
                column: "Day",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_game_sessions_Day",
                table: "game_sessions",
                column: "Day");

            migrationBuilder.CreateIndex(
                name: "IX_game_sessions_PlayerKey_Day",
                table: "game_sessions",
                columns: new[] { "PlayerKey", "Day" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_puzzle_rounds_PuzzleId_Index",
                table: "puzzle_rounds",
                columns: new[] { "PuzzleId", "Index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_puzzle_rounds_SongId",
                table: "puzzle_rounds",
                column: "SongId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "puzzle_rounds");

            migrationBuilder.DropTable(
                name: "round_progress");

            migrationBuilder.DropTable(
                name: "daily_puzzles");

            migrationBuilder.DropTable(
                name: "game_sessions");
        }
    }
}
