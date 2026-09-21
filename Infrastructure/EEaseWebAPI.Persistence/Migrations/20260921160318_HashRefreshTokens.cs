using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EEaseWebAPI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class HashRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RefreshToken",
                table: "AspNetUsers",
                newName: "RefreshTokenHash");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_RefreshTokenHash",
                table: "AspNetUsers",
                column: "RefreshTokenHash");

            // What is in the column is a token, not a hash of one, so it can never match
            // again. Leaving it would keep a readable token in the table for no purpose.
            // Everybody signs in again once; nobody is left holding a row that cannot work.
            migrationBuilder.Sql(
                @"UPDATE ""AspNetUsers"" SET ""RefreshTokenHash"" = NULL, ""RefreshTokenEndDate"" = NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_RefreshTokenHash",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "RefreshTokenHash",
                table: "AspNetUsers",
                newName: "RefreshToken");
        }
    }
}
