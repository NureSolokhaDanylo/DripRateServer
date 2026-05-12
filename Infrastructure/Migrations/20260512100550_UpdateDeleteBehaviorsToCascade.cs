using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDeleteBehaviorsToCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdvertisementTags_Advertisements_AdvertisementId",
                table: "AdvertisementTags");

            migrationBuilder.DropForeignKey(
                name: "FK_AdvertisementTags_Tags_TagId",
                table: "AdvertisementTags");

            migrationBuilder.DropForeignKey(
                name: "FK_PublicationClothes_Clothes_ClothId",
                table: "PublicationClothes");

            migrationBuilder.DropForeignKey(
                name: "FK_PublicationClothes_Publications_PublicationId",
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

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertisementTags_Advertisements_AdvertisementId",
                table: "AdvertisementTags",
                column: "AdvertisementId",
                principalTable: "Advertisements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertisementTags_Tags_TagId",
                table: "AdvertisementTags",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PublicationClothes_Clothes_ClothId",
                table: "PublicationClothes",
                column: "ClothId",
                principalTable: "Clothes",
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
                name: "FK_PublicationTags_Publications_PublicationId",
                table: "PublicationTags",
                column: "PublicationId",
                principalTable: "Publications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PublicationTags_Tags_TagId",
                table: "PublicationTags",
                column: "TagId",
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
                name: "FK_UserPreferredTags_Tags_TagId",
                table: "UserPreferredTags",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdvertisementTags_Advertisements_AdvertisementId",
                table: "AdvertisementTags");

            migrationBuilder.DropForeignKey(
                name: "FK_AdvertisementTags_Tags_TagId",
                table: "AdvertisementTags");

            migrationBuilder.DropForeignKey(
                name: "FK_PublicationClothes_Clothes_ClothId",
                table: "PublicationClothes");

            migrationBuilder.DropForeignKey(
                name: "FK_PublicationClothes_Publications_PublicationId",
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

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertisementTags_Advertisements_AdvertisementId",
                table: "AdvertisementTags",
                column: "AdvertisementId",
                principalTable: "Advertisements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertisementTags_Tags_TagId",
                table: "AdvertisementTags",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PublicationClothes_Clothes_ClothId",
                table: "PublicationClothes",
                column: "ClothId",
                principalTable: "Clothes",
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
    }
}
