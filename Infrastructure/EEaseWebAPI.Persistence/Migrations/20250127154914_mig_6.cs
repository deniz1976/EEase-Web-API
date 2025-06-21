using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EEaseWebAPI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig_6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAccommodationPreferences_AspNetUsers_UserId",
                table: "UserAccommodationPreferences");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFoodPreferences_AspNetUsers_UserId",
                table: "UserFoodPreferences");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserFoodPreferences",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserAccommodationPreferences",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "StandardRoutes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAccommodationPreferences_AspNetUsers_UserId",
                table: "UserAccommodationPreferences",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFoodPreferences_AspNetUsers_UserId",
                table: "UserFoodPreferences",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAccommodationPreferences_AspNetUsers_UserId",
                table: "UserAccommodationPreferences");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFoodPreferences_AspNetUsers_UserId",
                table: "UserFoodPreferences");

            migrationBuilder.DropColumn(
                name: "status",
                table: "StandardRoutes");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserFoodPreferences",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserAccommodationPreferences",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAccommodationPreferences_AspNetUsers_UserId",
                table: "UserAccommodationPreferences",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFoodPreferences_AspNetUsers_UserId",
                table: "UserFoodPreferences",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
