using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicationCounters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssessmentsCount",
                table: "Publications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "AverageRating",
                table: "Publications",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "CommentsCount",
                table: "Publications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LikesCount",
                table: "Publications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RatingColorSum",
                table: "Publications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RatingFitSum",
                table: "Publications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RatingOriginalitySum",
                table: "Publications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RatingStyleSum",
                table: "Publications",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssessmentsCount",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "CommentsCount",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "LikesCount",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "RatingColorSum",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "RatingFitSum",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "RatingOriginalitySum",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "RatingStyleSum",
                table: "Publications");
        }
    }
}
