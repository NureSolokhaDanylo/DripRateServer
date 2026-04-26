using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Indexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PublicationClothes_Clothes_ClothesId",
                table: "PublicationClothes");

            migrationBuilder.DropForeignKey(
                name: "FK_PublicationTags_Publications_PublicationId",
                table: "PublicationTags");

            migrationBuilder.DropForeignKey(
                name: "FK_PublicationTags_Tags_TagsId",
                table: "PublicationTags");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPreferredTags_AspNetUsers_UserId",
                table: "UserPreferredTags");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPreferredTags_Tags_PreferredTagsId",
                table: "UserPreferredTags");

            migrationBuilder.DropIndex(
                name: "IX_Comments_PublicationId",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "PreferredTagsId",
                table: "UserPreferredTags",
                newName: "TagId");

            migrationBuilder.RenameColumn(
                name: "TagsId",
                table: "PublicationTags",
                newName: "TagId");

            migrationBuilder.RenameIndex(
                name: "IX_PublicationTags_TagsId",
                table: "PublicationTags",
                newName: "IX_PublicationTags_TagId");

            migrationBuilder.RenameColumn(
                name: "ClothesId",
                table: "PublicationClothes",
                newName: "ClothId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferredTags_TagId",
                table: "UserPreferredTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Publications_CreatedAt",
                table: "Publications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PublicationClothes_ClothId",
                table: "PublicationClothes",
                column: "ClothId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_PublicationId_CreatedAt",
                table: "Comments",
                columns: new[] { "PublicationId", "CreatedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_PublicationClothes_Clothes_ClothId",
                table: "PublicationClothes",
                column: "ClothId",
                principalTable: "Clothes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PublicationTags_Publications_PublicationId",
                table: "PublicationTags",
                column: "PublicationId",
                principalTable: "Publications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PublicationTags_Tags_TagId",
                table: "PublicationTags",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreferredTags_AspNetUsers_UserId",
                table: "UserPreferredTags",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreferredTags_Tags_TagId",
                table: "UserPreferredTags",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PublicationClothes_Clothes_ClothId",
                table: "PublicationClothes");

            migrationBuilder.DropForeignKey(
                name: "FK_PublicationTags_Publications_PublicationId",
                table: "PublicationTags");

            migrationBuilder.DropForeignKey(
                name: "FK_PublicationTags_Tags_TagId",
                table: "PublicationTags");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPreferredTags_AspNetUsers_UserId",
                table: "UserPreferredTags");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPreferredTags_Tags_TagId",
                table: "UserPreferredTags");

            migrationBuilder.DropIndex(
                name: "IX_UserPreferredTags_TagId",
                table: "UserPreferredTags");

            migrationBuilder.DropIndex(
                name: "IX_Publications_CreatedAt",
                table: "Publications");

            migrationBuilder.DropIndex(
                name: "IX_PublicationClothes_ClothId",
                table: "PublicationClothes");

            migrationBuilder.DropIndex(
                name: "IX_Comments_PublicationId_CreatedAt",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "TagId",
                table: "UserPreferredTags",
                newName: "PreferredTagsId");

            migrationBuilder.RenameColumn(
                name: "TagId",
                table: "PublicationTags",
                newName: "TagsId");

            migrationBuilder.RenameIndex(
                name: "IX_PublicationTags_TagId",
                table: "PublicationTags",
                newName: "IX_PublicationTags_TagsId");

            migrationBuilder.RenameColumn(
                name: "ClothId",
                table: "PublicationClothes",
                newName: "ClothesId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_PublicationId",
                table: "Comments",
                column: "PublicationId");

            migrationBuilder.AddForeignKey(
                name: "FK_PublicationClothes_Clothes_ClothesId",
                table: "PublicationClothes",
                column: "ClothesId",
                principalTable: "Clothes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PublicationTags_Publications_PublicationId",
                table: "PublicationTags",
                column: "PublicationId",
                principalTable: "Publications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PublicationTags_Tags_TagsId",
                table: "PublicationTags",
                column: "TagsId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreferredTags_AspNetUsers_UserId",
                table: "UserPreferredTags",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreferredTags_Tags_PreferredTagsId",
                table: "UserPreferredTags",
                column: "PreferredTagsId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
