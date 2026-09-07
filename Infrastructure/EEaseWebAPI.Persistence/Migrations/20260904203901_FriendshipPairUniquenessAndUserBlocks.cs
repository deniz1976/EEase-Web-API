using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EEaseWebAPI.Persistence.Migrations
{
    public partial class FriendshipPairUniquenessAndUserBlocks : Migration
    {
        private const string PairOrdering = @"
            UPDATE ""UserFriendships""
            SET ""UserAId"" = CASE
                    WHEN ""RequesterId"" COLLATE ""C"" <= ""AddresseeId"" COLLATE ""C""
                    THEN ""RequesterId"" ELSE ""AddresseeId"" END,
                ""UserBId"" = CASE
                    WHEN ""RequesterId"" COLLATE ""C"" <= ""AddresseeId"" COLLATE ""C""
                    THEN ""AddresseeId"" ELSE ""RequesterId"" END;
        ";

        private const string CopyBlocksOut = @"
            INSERT INTO ""UserBlocks""
                (""Id"", ""BlockerId"", ""BlockedId"", ""BlockedDate"", ""CreatedDate"", ""UpdatedDate"")
            SELECT gen_random_uuid(),
                   ""RequesterId"",
                   ""AddresseeId"",
                   COALESCE(""ResponseDate"", ""RequestDate""),
                   ""CreatedDate"",
                   ""UpdatedDate""
            FROM ""UserFriendships""
            WHERE ""Status"" = 3
            ON CONFLICT DO NOTHING;
        ";

        private const string DropBlockedFriendships = @"
            DELETE FROM ""UserFriendships"" WHERE ""Status"" = 3;
        ";

        private const string DeduplicatePairs = @"
            DELETE FROM ""UserFriendships"" f
            USING (
                SELECT ""Id"",
                       ROW_NUMBER() OVER (
                           PARTITION BY ""UserAId"", ""UserBId""
                           ORDER BY CASE ""Status""
                                        WHEN 1 THEN 0
                                        WHEN 0 THEN 1
                                        ELSE 2
                                    END,
                                    ""RequestDate"" DESC
                       ) AS row_rank
                FROM ""UserFriendships""
            ) ranked
            WHERE f.""Id"" = ranked.""Id"" AND ranked.row_rank > 1;
        ";

        private const string CopyBlocksBack = @"
            INSERT INTO ""UserFriendships""
                (""Id"", ""RequesterId"", ""AddresseeId"", ""Status"", ""RequestDate"", ""ResponseDate"", ""CreatedDate"", ""UpdatedDate"")
            SELECT gen_random_uuid(),
                   ""BlockerId"",
                   ""BlockedId"",
                   3,
                   ""BlockedDate"",
                   ""BlockedDate"",
                   ""CreatedDate"",
                   ""UpdatedDate""
            FROM ""UserBlocks""
            ON CONFLICT DO NOTHING;
        ";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserFriendships_RequesterId_AddresseeId",
                table: "UserFriendships");

            migrationBuilder.AddColumn<string>(
                name: "UserAId",
                table: "UserFriendships",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserBId",
                table: "UserFriendships",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "UserBlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockerId = table.Column<string>(type: "text", nullable: false),
                    BlockedId = table.Column<string>(type: "text", nullable: false),
                    BlockedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBlocks_AspNetUsers_BlockedId",
                        column: x => x.BlockedId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserBlocks_AspNetUsers_BlockerId",
                        column: x => x.BlockerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserBlocks_BlockedId",
                table: "UserBlocks",
                column: "BlockedId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBlocks_BlockerId_BlockedId",
                table: "UserBlocks",
                columns: new[] { "BlockerId", "BlockedId" },
                unique: true);

            migrationBuilder.Sql(CopyBlocksOut);
            migrationBuilder.Sql(DropBlockedFriendships);
            migrationBuilder.Sql(PairOrdering);
            migrationBuilder.Sql(DeduplicatePairs);

            migrationBuilder.CreateIndex(
                name: "IX_UserFriendships_UserAId_UserBId",
                table: "UserFriendships",
                columns: new[] { "UserAId", "UserBId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserFriendships_UserAId_UserBId",
                table: "UserFriendships");

            migrationBuilder.CreateIndex(
                name: "IX_UserFriendships_RequesterId_AddresseeId",
                table: "UserFriendships",
                columns: new[] { "RequesterId", "AddresseeId" },
                unique: true);

            migrationBuilder.Sql(CopyBlocksBack);

            migrationBuilder.DropTable(
                name: "UserBlocks");

            migrationBuilder.DropColumn(
                name: "UserAId",
                table: "UserFriendships");

            migrationBuilder.DropColumn(
                name: "UserBId",
                table: "UserFriendships");
        }
    }
}
