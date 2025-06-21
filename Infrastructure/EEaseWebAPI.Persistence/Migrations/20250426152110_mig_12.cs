using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EEaseWebAPI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig_12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "approxPrice",
                table: "TravelDays",
                type: "text",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "approxPrice",
                table: "TravelDays",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
