using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFirstImpressionEnabled",
                table: "Publications",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsGuessPriceEnabled",
                table: "Publications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTagMatchEnabled",
                table: "Publications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "GameStats",
                columns: table => new
                {
                    PublicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GuessPriceTotalCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    GuessPriceRealSum = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    GuessPriceGuessedSum = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    FirstImpressionTotalCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FirstImpressionPositiveCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FirstImpressionReactionTimeSum = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    TagMatchTotalCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TagMatchCorrectCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameStats", x => x.PublicationId);
                    table.ForeignKey(
                        name: "FK_GameStats_Publications_PublicationId",
                        column: x => x.PublicationId,
                        principalTable: "Publications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserGameCursors",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameType = table.Column<int>(type: "int", nullable: false),
                    LastSeenCreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGameCursors", x => new { x.UserId, x.GameType });
                    table.ForeignKey(
                        name: "FK_UserGameCursors_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Publications_AverageRating_AssessmentsCount_Id",
                table: "Publications",
                columns: new[] { "AverageRating", "AssessmentsCount", "Id" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Publications_CreatedAt_Id",
                table: "Publications",
                columns: new[] { "CreatedAt", "Id" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Collections_IsPublic_CreatedAt_Id",
                table: "Collections",
                columns: new[] { "IsPublic", "CreatedAt", "Id" },
                descending: new[] { false, true, false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameStats");

            migrationBuilder.DropTable(
                name: "UserGameCursors");

            migrationBuilder.DropIndex(
                name: "IX_Publications_AverageRating_AssessmentsCount_Id",
                table: "Publications");

            migrationBuilder.DropIndex(
                name: "IX_Publications_CreatedAt_Id",
                table: "Publications");

            migrationBuilder.DropIndex(
                name: "IX_Collections_IsPublic_CreatedAt_Id",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "IsFirstImpressionEnabled",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "IsGuessPriceEnabled",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "IsTagMatchEnabled",
                table: "Publications");
        }
    }
}
