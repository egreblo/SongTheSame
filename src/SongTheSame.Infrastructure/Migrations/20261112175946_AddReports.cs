using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SongTheSame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "round_reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoundIndex = table.Column<int>(type: "integer", nullable: false),
                    TargetSongId = table.Column<Guid>(type: "uuid", nullable: false),
                    GuessedSongId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_round_reports", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_round_reports_SessionId_RoundIndex",
                table: "round_reports",
                columns: new[] { "SessionId", "RoundIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_round_reports_Status",
                table: "round_reports",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "round_reports");
        }
    }
}
