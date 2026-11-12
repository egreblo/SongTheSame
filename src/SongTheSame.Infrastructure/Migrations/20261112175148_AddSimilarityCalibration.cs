using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SongTheSame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSimilarityCalibration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "similarity_calibrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExtractorVersion = table.Column<int>(type: "integer", nullable: false),
                    SortedCosts = table.Column<float[]>(type: "real[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_similarity_calibrations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_similarity_calibrations_CreatedAt",
                table: "similarity_calibrations",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "similarity_calibrations");
        }
    }
}
