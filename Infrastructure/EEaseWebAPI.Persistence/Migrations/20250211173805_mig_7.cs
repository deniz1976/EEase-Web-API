using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EEaseWebAPI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig_7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserAccomodationPreference",
                table: "TravelAccomodations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserPersonalizationPref",
                table: "BaseTravelPlaceEntity",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserFoodPreference",
                table: "BaseRestaurantPlaceEntity",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserAccomodationPreference",
                table: "TravelAccomodations");

            migrationBuilder.DropColumn(
                name: "UserPersonalizationPref",
                table: "BaseTravelPlaceEntity");

            migrationBuilder.DropColumn(
                name: "UserFoodPreference",
                table: "BaseRestaurantPlaceEntity");
        }
    }
}
