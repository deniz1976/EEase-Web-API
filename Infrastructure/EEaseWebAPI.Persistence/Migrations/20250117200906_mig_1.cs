using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EEaseWebAPI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "AllWorldCities",
            //    columns: table => new
            //    {
            //        city = table.Column<string>(type: "text", nullable: true),
            //        city_ascii = table.Column<string>(type: "text", nullable: true),
            //        lat = table.Column<double>(type: "double precision", nullable: true),
            //        lng = table.Column<double>(type: "double precision", nullable: true),
            //        country = table.Column<string>(type: "text", nullable: true),
            //        iso2 = table.Column<string>(type: "text", nullable: true),
            //        iso3 = table.Column<string>(type: "text", nullable: true),
            //        admin_name = table.Column<string>(type: "text", nullable: true),
            //        capital = table.Column<string>(type: "text", nullable: true),
            //        population = table.Column<double>(type: "double precision", nullable: true),
            //        id = table.Column<int>(type: "integer", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AspNetRoles",
            //    columns: table => new
            //    {
            //        Id = table.Column<string>(type: "text", nullable: false),
            //        Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
            //        NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
            //        ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetRoles", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AspNetUsers",
            //    columns: table => new
            //    {
            //        Id = table.Column<string>(type: "text", nullable: false),
            //        Name = table.Column<string>(type: "text", nullable: true),
            //        Surname = table.Column<string>(type: "text", nullable: true),
            //        Gender = table.Column<string>(type: "text", nullable: true),
            //        BornDate = table.Column<DateOnly>(type: "date", nullable: true),
            //        DeleteDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
            //        DeleteCode = table.Column<string>(type: "text", nullable: true),
            //        RefreshToken = table.Column<string>(type: "text", nullable: true),
            //        RefreshTokenEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
            //        VerificationCode = table.Column<string>(type: "text", nullable: true),
            //        ResetPasswordCode = table.Column<string>(type: "text", nullable: true),
            //        status = table.Column<bool>(type: "boolean", nullable: true),
            //        Country = table.Column<string>(type: "text", nullable: true),
            //        UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
            //        NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
            //        Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
            //        NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
            //        EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
            //        PasswordHash = table.Column<string>(type: "text", nullable: true),
            //        SecurityStamp = table.Column<string>(type: "text", nullable: true),
            //        ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
            //        PhoneNumber = table.Column<string>(type: "text", nullable: true),
            //        PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
            //        TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
            //        LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            //        LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
            //        AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetUsers", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Currencies",
            //    columns: table => new
            //    {
            //        Entity = table.Column<string>(type: "text", nullable: true),
            //        Currency = table.Column<string>(type: "text", nullable: true),
            //        AlphabeticCode = table.Column<string>(type: "text", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //    });

            //migrationBuilder.CreateTable(
            //    name: "StandardRoutes",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uuid", nullable: false),
            //        name = table.Column<string>(type: "text", nullable: true),
            //        CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            //        UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_StandardRoutes", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AspNetRoleClaims",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "integer", nullable: false)
            //            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            //        RoleId = table.Column<string>(type: "text", nullable: false),
            //        ClaimType = table.Column<string>(type: "text", nullable: true),
            //        ClaimValue = table.Column<string>(type: "text", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
            //            column: x => x.RoleId,
            //            principalTable: "AspNetRoles",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AspNetUserClaims",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "integer", nullable: false)
            //            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            //        UserId = table.Column<string>(type: "text", nullable: false),
            //        ClaimType = table.Column<string>(type: "text", nullable: true),
            //        ClaimValue = table.Column<string>(type: "text", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_AspNetUserClaims_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AspNetUserLogins",
            //    columns: table => new
            //    {
            //        LoginProvider = table.Column<string>(type: "text", nullable: false),
            //        ProviderKey = table.Column<string>(type: "text", nullable: false),
            //        ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
            //        UserId = table.Column<string>(type: "text", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
            //        table.ForeignKey(
            //            name: "FK_AspNetUserLogins_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AspNetUserRoles",
            //    columns: table => new
            //    {
            //        UserId = table.Column<string>(type: "text", nullable: false),
            //        RoleId = table.Column<string>(type: "text", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
            //        table.ForeignKey(
            //            name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
            //            column: x => x.RoleId,
            //            principalTable: "AspNetRoles",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_AspNetUserRoles_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AspNetUserTokens",
            //    columns: table => new
            //    {
            //        UserId = table.Column<string>(type: "text", nullable: false),
            //        LoginProvider = table.Column<string>(type: "text", nullable: false),
            //        Name = table.Column<string>(type: "text", nullable: false),
            //        Value = table.Column<string>(type: "text", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
            //        table.ForeignKey(
            //            name: "FK_AspNetUserTokens_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            migrationBuilder.CreateTable(
                name: "UserAccommodationPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LuxuryHotelPreference = table.Column<int>(type: "integer", nullable: true),
                    BudgetHotelPreference = table.Column<int>(type: "integer", nullable: true),
                    BoutiqueHotelPreference = table.Column<int>(type: "integer", nullable: true),
                    HostelPreference = table.Column<int>(type: "integer", nullable: true),
                    ApartmentPreference = table.Column<int>(type: "integer", nullable: true),
                    ResortPreference = table.Column<int>(type: "integer", nullable: true),
                    VillaPreference = table.Column<int>(type: "integer", nullable: true),
                    GuestHousePreference = table.Column<int>(type: "integer", nullable: true),
                    CampingPreference = table.Column<int>(type: "integer", nullable: true),
                    GlampingPreference = table.Column<int>(type: "integer", nullable: true),
                    BedAndBreakfastPreference = table.Column<int>(type: "integer", nullable: true),
                    AllInclusivePreference = table.Column<int>(type: "integer", nullable: true),
                    SpaAndWellnessPreference = table.Column<int>(type: "integer", nullable: true),
                    PetFriendlyPreference = table.Column<int>(type: "integer", nullable: true),
                    EcoFriendlyPreference = table.Column<int>(type: "integer", nullable: true),
                    RemoteLocationPreference = table.Column<int>(type: "integer", nullable: true),
                    CityCenterPreference = table.Column<int>(type: "integer", nullable: true),
                    FamilyFriendlyPreference = table.Column<int>(type: "integer", nullable: true),
                    AdultsOnlyPreference = table.Column<int>(type: "integer", nullable: true),
                    HomestayPreference = table.Column<int>(type: "integer", nullable: true),
                    WaterfrontPreference = table.Column<int>(type: "integer", nullable: true),
                    HistoricalBuildingPreference = table.Column<int>(type: "integer", nullable: true),
                    AirbnbPreference = table.Column<int>(type: "integer", nullable: true),
                    CoLivingSpacePreference = table.Column<int>(type: "integer", nullable: true),
                    ExtendedStayPreference = table.Column<int>(type: "integer", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccommodationPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAccommodationPreferences_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFoodPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    VegetarianPreference = table.Column<int>(type: "integer", nullable: true),
                    VeganPreference = table.Column<int>(type: "integer", nullable: true),
                    GlutenFreePreference = table.Column<int>(type: "integer", nullable: true),
                    HalalPreference = table.Column<int>(type: "integer", nullable: true),
                    KosherPreference = table.Column<int>(type: "integer", nullable: true),
                    SeafoodPreference = table.Column<int>(type: "integer", nullable: true),
                    LocalCuisinePreference = table.Column<int>(type: "integer", nullable: true),
                    FastFoodPreference = table.Column<int>(type: "integer", nullable: true),
                    FinePreference = table.Column<int>(type: "integer", nullable: true),
                    StreetFoodPreference = table.Column<int>(type: "integer", nullable: true),
                    OrganicPreference = table.Column<int>(type: "integer", nullable: true),
                    BuffetPreference = table.Column<int>(type: "integer", nullable: true),
                    FoodTruckPreference = table.Column<int>(type: "integer", nullable: true),
                    CafeteriaPreference = table.Column<int>(type: "integer", nullable: true),
                    DeliveryPreference = table.Column<int>(type: "integer", nullable: true),
                    AllergiesPreference = table.Column<int>(type: "integer", nullable: true),
                    DairyFreePreference = table.Column<int>(type: "integer", nullable: true),
                    NutFreePreference = table.Column<int>(type: "integer", nullable: true),
                    SpicyPreference = table.Column<int>(type: "integer", nullable: true),
                    SweetPreference = table.Column<int>(type: "integer", nullable: true),
                    SaltyPreference = table.Column<int>(type: "integer", nullable: true),
                    SourPreference = table.Column<int>(type: "integer", nullable: true),
                    BitterPreference = table.Column<int>(type: "integer", nullable: true),
                    UmamiPreference = table.Column<int>(type: "integer", nullable: true),
                    FusionPreference = table.Column<int>(type: "integer", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFoodPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFoodPreferences_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPersonalizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    AdventurePreference = table.Column<int>(type: "integer", nullable: true),
                    RelaxationPreference = table.Column<int>(type: "integer", nullable: true),
                    CulturalPreference = table.Column<int>(type: "integer", nullable: true),
                    NaturePreference = table.Column<int>(type: "integer", nullable: true),
                    UrbanPreference = table.Column<int>(type: "integer", nullable: true),
                    RuralPreference = table.Column<int>(type: "integer", nullable: true),
                    LuxuryPreference = table.Column<int>(type: "integer", nullable: true),
                    BudgetPreference = table.Column<int>(type: "integer", nullable: true),
                    SoloTravelPreference = table.Column<int>(type: "integer", nullable: true),
                    GroupTravelPreference = table.Column<int>(type: "integer", nullable: true),
                    FamilyTravelPreference = table.Column<int>(type: "integer", nullable: true),
                    CoupleTravelPreference = table.Column<int>(type: "integer", nullable: true),
                    BeachPreference = table.Column<int>(type: "integer", nullable: true),
                    MountainPreference = table.Column<int>(type: "integer", nullable: true),
                    DesertPreference = table.Column<int>(type: "integer", nullable: true),
                    ForestPreference = table.Column<int>(type: "integer", nullable: true),
                    IslandPreference = table.Column<int>(type: "integer", nullable: true),
                    LakePreference = table.Column<int>(type: "integer", nullable: true),
                    RiverPreference = table.Column<int>(type: "integer", nullable: true),
                    WaterfallPreference = table.Column<int>(type: "integer", nullable: true),
                    CavePreference = table.Column<int>(type: "integer", nullable: true),
                    VolcanoPreference = table.Column<int>(type: "integer", nullable: true),
                    GlacierPreference = table.Column<int>(type: "integer", nullable: true),
                    CanyonPreference = table.Column<int>(type: "integer", nullable: true),
                    ValleyPreference = table.Column<int>(type: "integer", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPersonalizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPersonalizations_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            //migrationBuilder.CreateIndex(
            //    name: "IX_AspNetRoleClaims_RoleId",
            //    table: "AspNetRoleClaims",
            //    column: "RoleId");

            //migrationBuilder.CreateIndex(
            //    name: "RoleNameIndex",
            //    table: "AspNetRoles",
            //    column: "NormalizedName",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_AspNetUserClaims_UserId",
            //    table: "AspNetUserClaims",
            //    column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AspNetUserLogins_UserId",
            //    table: "AspNetUserLogins",
            //    column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AspNetUserRoles_RoleId",
            //    table: "AspNetUserRoles",
            //    column: "RoleId");

            //migrationBuilder.CreateIndex(
            //    name: "EmailIndex",
            //    table: "AspNetUsers",
            //    column: "NormalizedEmail");

            //migrationBuilder.CreateIndex(
            //    name: "UserNameIndex",
            //    table: "AspNetUsers",
            //    column: "NormalizedUserName",
            //    unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAccommodationPreferences_UserId",
                table: "UserAccommodationPreferences",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserFoodPreferences_UserId",
                table: "UserFoodPreferences",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPersonalizations_UserId",
                table: "UserPersonalizations",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllWorldCities");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "StandardRoutes");

            migrationBuilder.DropTable(
                name: "UserAccommodationPreferences");

            migrationBuilder.DropTable(
                name: "UserFoodPreferences");

            migrationBuilder.DropTable(
                name: "UserPersonalizations");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
