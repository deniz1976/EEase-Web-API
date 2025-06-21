using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EEaseWebAPI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig_4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPersonalizations_AspNetUsers_UserId",
                table: "UserPersonalizations");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserPersonalizations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "UserAccommodationPreferencesId",
                table: "StandardRoutes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserFoodPreferencesId",
                table: "StandardRoutes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserPersonalizationId",
                table: "StandardRoutes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserAccommodationPreferencesId",
                table: "BaseTravelPlaceEntity",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserFoodPreferencesId",
                table: "BaseTravelPlaceEntity",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserPersonalizationId",
                table: "BaseTravelPlaceEntity",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserAccommodationPreferencesId",
                table: "BaseRestaurantPlaceEntity",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserFoodPreferencesId",
                table: "BaseRestaurantPlaceEntity",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserPersonalizationId",
                table: "BaseRestaurantPlaceEntity",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StandardRoutes_UserAccommodationPreferencesId",
                table: "StandardRoutes",
                column: "UserAccommodationPreferencesId");

            migrationBuilder.CreateIndex(
                name: "IX_StandardRoutes_UserFoodPreferencesId",
                table: "StandardRoutes",
                column: "UserFoodPreferencesId");

            migrationBuilder.CreateIndex(
                name: "IX_StandardRoutes_UserPersonalizationId",
                table: "StandardRoutes",
                column: "UserPersonalizationId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseTravelPlaceEntity_UserAccommodationPreferencesId",
                table: "BaseTravelPlaceEntity",
                column: "UserAccommodationPreferencesId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseTravelPlaceEntity_UserFoodPreferencesId",
                table: "BaseTravelPlaceEntity",
                column: "UserFoodPreferencesId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseTravelPlaceEntity_UserPersonalizationId",
                table: "BaseTravelPlaceEntity",
                column: "UserPersonalizationId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseRestaurantPlaceEntity_UserAccommodationPreferencesId",
                table: "BaseRestaurantPlaceEntity",
                column: "UserAccommodationPreferencesId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseRestaurantPlaceEntity_UserFoodPreferencesId",
                table: "BaseRestaurantPlaceEntity",
                column: "UserFoodPreferencesId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseRestaurantPlaceEntity_UserPersonalizationId",
                table: "BaseRestaurantPlaceEntity",
                column: "UserPersonalizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_BaseRestaurantPlaceEntity_UserAccommodationPreferences_User~",
                table: "BaseRestaurantPlaceEntity",
                column: "UserAccommodationPreferencesId",
                principalTable: "UserAccommodationPreferences",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BaseRestaurantPlaceEntity_UserFoodPreferences_UserFoodPrefe~",
                table: "BaseRestaurantPlaceEntity",
                column: "UserFoodPreferencesId",
                principalTable: "UserFoodPreferences",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BaseRestaurantPlaceEntity_UserPersonalizations_UserPersonal~",
                table: "BaseRestaurantPlaceEntity",
                column: "UserPersonalizationId",
                principalTable: "UserPersonalizations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BaseTravelPlaceEntity_UserAccommodationPreferences_UserAcco~",
                table: "BaseTravelPlaceEntity",
                column: "UserAccommodationPreferencesId",
                principalTable: "UserAccommodationPreferences",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BaseTravelPlaceEntity_UserFoodPreferences_UserFoodPreferenc~",
                table: "BaseTravelPlaceEntity",
                column: "UserFoodPreferencesId",
                principalTable: "UserFoodPreferences",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BaseTravelPlaceEntity_UserPersonalizations_UserPersonalizat~",
                table: "BaseTravelPlaceEntity",
                column: "UserPersonalizationId",
                principalTable: "UserPersonalizations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StandardRoutes_UserAccommodationPreferences_UserAccommodati~",
                table: "StandardRoutes",
                column: "UserAccommodationPreferencesId",
                principalTable: "UserAccommodationPreferences",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StandardRoutes_UserFoodPreferences_UserFoodPreferencesId",
                table: "StandardRoutes",
                column: "UserFoodPreferencesId",
                principalTable: "UserFoodPreferences",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StandardRoutes_UserPersonalizations_UserPersonalizationId",
                table: "StandardRoutes",
                column: "UserPersonalizationId",
                principalTable: "UserPersonalizations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPersonalizations_AspNetUsers_UserId",
                table: "UserPersonalizations",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaseRestaurantPlaceEntity_UserAccommodationPreferences_User~",
                table: "BaseRestaurantPlaceEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_BaseRestaurantPlaceEntity_UserFoodPreferences_UserFoodPrefe~",
                table: "BaseRestaurantPlaceEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_BaseRestaurantPlaceEntity_UserPersonalizations_UserPersonal~",
                table: "BaseRestaurantPlaceEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_BaseTravelPlaceEntity_UserAccommodationPreferences_UserAcco~",
                table: "BaseTravelPlaceEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_BaseTravelPlaceEntity_UserFoodPreferences_UserFoodPreferenc~",
                table: "BaseTravelPlaceEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_BaseTravelPlaceEntity_UserPersonalizations_UserPersonalizat~",
                table: "BaseTravelPlaceEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_StandardRoutes_UserAccommodationPreferences_UserAccommodati~",
                table: "StandardRoutes");

            migrationBuilder.DropForeignKey(
                name: "FK_StandardRoutes_UserFoodPreferences_UserFoodPreferencesId",
                table: "StandardRoutes");

            migrationBuilder.DropForeignKey(
                name: "FK_StandardRoutes_UserPersonalizations_UserPersonalizationId",
                table: "StandardRoutes");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPersonalizations_AspNetUsers_UserId",
                table: "UserPersonalizations");

            migrationBuilder.DropIndex(
                name: "IX_StandardRoutes_UserAccommodationPreferencesId",
                table: "StandardRoutes");

            migrationBuilder.DropIndex(
                name: "IX_StandardRoutes_UserFoodPreferencesId",
                table: "StandardRoutes");

            migrationBuilder.DropIndex(
                name: "IX_StandardRoutes_UserPersonalizationId",
                table: "StandardRoutes");

            migrationBuilder.DropIndex(
                name: "IX_BaseTravelPlaceEntity_UserAccommodationPreferencesId",
                table: "BaseTravelPlaceEntity");

            migrationBuilder.DropIndex(
                name: "IX_BaseTravelPlaceEntity_UserFoodPreferencesId",
                table: "BaseTravelPlaceEntity");

            migrationBuilder.DropIndex(
                name: "IX_BaseTravelPlaceEntity_UserPersonalizationId",
                table: "BaseTravelPlaceEntity");

            migrationBuilder.DropIndex(
                name: "IX_BaseRestaurantPlaceEntity_UserAccommodationPreferencesId",
                table: "BaseRestaurantPlaceEntity");

            migrationBuilder.DropIndex(
                name: "IX_BaseRestaurantPlaceEntity_UserFoodPreferencesId",
                table: "BaseRestaurantPlaceEntity");

            migrationBuilder.DropIndex(
                name: "IX_BaseRestaurantPlaceEntity_UserPersonalizationId",
                table: "BaseRestaurantPlaceEntity");

            migrationBuilder.DropColumn(
                name: "UserAccommodationPreferencesId",
                table: "StandardRoutes");

            migrationBuilder.DropColumn(
                name: "UserFoodPreferencesId",
                table: "StandardRoutes");

            migrationBuilder.DropColumn(
                name: "UserPersonalizationId",
                table: "StandardRoutes");

            migrationBuilder.DropColumn(
                name: "UserAccommodationPreferencesId",
                table: "BaseTravelPlaceEntity");

            migrationBuilder.DropColumn(
                name: "UserFoodPreferencesId",
                table: "BaseTravelPlaceEntity");

            migrationBuilder.DropColumn(
                name: "UserPersonalizationId",
                table: "BaseTravelPlaceEntity");

            migrationBuilder.DropColumn(
                name: "UserAccommodationPreferencesId",
                table: "BaseRestaurantPlaceEntity");

            migrationBuilder.DropColumn(
                name: "UserFoodPreferencesId",
                table: "BaseRestaurantPlaceEntity");

            migrationBuilder.DropColumn(
                name: "UserPersonalizationId",
                table: "BaseRestaurantPlaceEntity");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserPersonalizations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPersonalizations_AspNetUsers_UserId",
                table: "UserPersonalizations",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
