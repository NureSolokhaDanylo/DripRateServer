using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeleteRestrict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assessments_Publications_PublicationId",
                table: "Assessments");

            migrationBuilder.DropForeignKey(
                name: "FK_Clothes_AspNetUsers_UserId",
                table: "Clothes");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Publications_PublicationId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_PublicationClothes_Publications_PublicationId",
                table: "PublicationClothes");

            migrationBuilder.DropForeignKey(
                name: "FK_Publications_AspNetUsers_UserId",
                table: "Publications");

            migrationBuilder.AddForeignKey(
                name: "FK_Assessments_Publications_PublicationId",
                table: "Assessments",
                column: "PublicationId",
                principalTable: "Publications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Clothes_AspNetUsers_UserId",
                table: "Clothes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Publications_PublicationId",
                table: "Comments",
                column: "PublicationId",
                principalTable: "Publications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PublicationClothes_Publications_PublicationId",
                table: "PublicationClothes",
                column: "PublicationId",
                principalTable: "Publications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Publications_AspNetUsers_UserId",
                table: "Publications",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assessments_Publications_PublicationId",
                table: "Assessments");

            migrationBuilder.DropForeignKey(
                name: "FK_Clothes_AspNetUsers_UserId",
                table: "Clothes");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Publications_PublicationId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_PublicationClothes_Publications_PublicationId",
                table: "PublicationClothes");

            migrationBuilder.DropForeignKey(
                name: "FK_Publications_AspNetUsers_UserId",
                table: "Publications");

            migrationBuilder.AddForeignKey(
                name: "FK_Assessments_Publications_PublicationId",
                table: "Assessments",
                column: "PublicationId",
                principalTable: "Publications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Clothes_AspNetUsers_UserId",
                table: "Clothes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Publications_PublicationId",
                table: "Comments",
                column: "PublicationId",
                principalTable: "Publications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PublicationClothes_Publications_PublicationId",
                table: "PublicationClothes",
                column: "PublicationId",
                principalTable: "Publications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Publications_AspNetUsers_UserId",
                table: "Publications",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
