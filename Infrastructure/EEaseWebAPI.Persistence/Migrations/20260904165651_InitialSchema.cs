using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EEaseWebAPI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AllWorldCities",
                columns: table => new
                {
                    city = table.Column<string>(type: "text", nullable: true),
                    city_ascii = table.Column<string>(type: "text", nullable: true),
                    lat = table.Column<double>(type: "double precision", nullable: true),
                    lng = table.Column<double>(type: "double precision", nullable: true),
                    country = table.Column<string>(type: "text", nullable: true),
                    iso2 = table.Column<string>(type: "text", nullable: true),
                    iso3 = table.Column<string>(type: "text", nullable: true),
                    admin_name = table.Column<string>(type: "text", nullable: true),
                    capital = table.Column<string>(type: "text", nullable: true),
                    population = table.Column<double>(type: "double precision", nullable: true),
                    id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Surname = table.Column<string>(type: "text", nullable: true),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    BornDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteCode = table.Column<string>(type: "text", nullable: true),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerificationCode = table.Column<string>(type: "text", nullable: true),
                    ResetPasswordCode = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<bool>(type: "boolean", nullable: true),
                    Currency = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true),
                    Bio = table.Column<string>(type: "text", nullable: true),
                    PhotoPath = table.Column<string>(type: "text", nullable: true),
                    LastSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Entity = table.Column<string>(type: "text", nullable: true),
                    Currency = table.Column<string>(type: "text", nullable: true),
                    AlphabeticCode = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "Weathers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Degree = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Warning = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Weathers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAccommodationPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
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
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserFoodPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
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
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserFriendships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterId = table.Column<string>(type: "text", nullable: false),
                    AddresseeId = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResponseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFriendships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFriendships_AspNetUsers_AddresseeId",
                        column: x => x.AddresseeId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserFriendships_AspNetUsers_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPersonalizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
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
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BaseRestaurantPlaceEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NationalPhoneNumber = table.Column<string>(type: "text", nullable: true),
                    FormattedAddress = table.Column<string>(type: "text", nullable: true),
                    Rating = table.Column<double>(type: "double precision", nullable: true),
                    GoogleMapsUri = table.Column<string>(type: "text", nullable: true),
                    WebsiteUri = table.Column<string>(type: "text", nullable: true),
                    PrimaryType = table.Column<string>(type: "text", nullable: true),
                    GoogleId = table.Column<string>(type: "text", nullable: true),
                    Reservable = table.Column<bool>(type: "boolean", nullable: true),
                    ServesBrunch = table.Column<bool>(type: "boolean", nullable: true),
                    ServesVegetarianFood = table.Column<bool>(type: "boolean", nullable: true),
                    ShortFormattedAddress = table.Column<string>(type: "text", nullable: true),
                    OutdoorSeating = table.Column<bool>(type: "boolean", nullable: true),
                    LiveMusic = table.Column<bool>(type: "boolean", nullable: true),
                    MenuForChildren = table.Column<bool>(type: "boolean", nullable: true),
                    Restroom = table.Column<bool>(type: "boolean", nullable: true),
                    GoodForGroups = table.Column<bool>(type: "boolean", nullable: true),
                    _PRICE_LEVEL = table.Column<int>(type: "integer", nullable: true),
                    WeatherId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserFoodPreferencesId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserPersonalizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserAccommodationPreferencesId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserFoodPreference = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseRestaurantPlaceEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaseRestaurantPlaceEntity_UserAccommodationPreferences_User~",
                        column: x => x.UserAccommodationPreferencesId,
                        principalTable: "UserAccommodationPreferences",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BaseRestaurantPlaceEntity_UserFoodPreferences_UserFoodPrefe~",
                        column: x => x.UserFoodPreferencesId,
                        principalTable: "UserFoodPreferences",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BaseRestaurantPlaceEntity_UserPersonalizations_UserPersonal~",
                        column: x => x.UserPersonalizationId,
                        principalTable: "UserPersonalizations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BaseRestaurantPlaceEntity_Weathers_WeatherId",
                        column: x => x.WeatherId,
                        principalTable: "Weathers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BaseTravelPlaceEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NationalPhoneNumber = table.Column<string>(type: "text", nullable: true),
                    FormattedAddress = table.Column<string>(type: "text", nullable: true),
                    Rating = table.Column<double>(type: "double precision", nullable: true),
                    GoogleMapsUri = table.Column<string>(type: "text", nullable: true),
                    WebsiteUri = table.Column<string>(type: "text", nullable: true),
                    GoodForChildren = table.Column<bool>(type: "boolean", nullable: true),
                    Restroom = table.Column<bool>(type: "boolean", nullable: true),
                    PrimaryType = table.Column<string>(type: "text", nullable: true),
                    GoogleId = table.Column<string>(type: "text", nullable: true),
                    _PRICE_LEVEL = table.Column<int>(type: "integer", nullable: true),
                    UserFoodPreferencesId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserPersonalizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserAccommodationPreferencesId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserPersonalizationPref = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseTravelPlaceEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaseTravelPlaceEntity_UserAccommodationPreferences_UserAcco~",
                        column: x => x.UserAccommodationPreferencesId,
                        principalTable: "UserAccommodationPreferences",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BaseTravelPlaceEntity_UserFoodPreferences_UserFoodPreferenc~",
                        column: x => x.UserFoodPreferencesId,
                        principalTable: "UserFoodPreferences",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BaseTravelPlaceEntity_UserPersonalizations_UserPersonalizat~",
                        column: x => x.UserPersonalizationId,
                        principalTable: "UserPersonalizations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StandardRoutes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    Days = table.Column<int>(type: "integer", nullable: true),
                    LikeCount = table.Column<int>(type: "integer", nullable: true),
                    Currency = table.Column<string>(type: "text", nullable: true),
                    UserFoodPreferencesId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserPersonalizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserAccommodationPreferencesId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StandardRoutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StandardRoutes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StandardRoutes_UserAccommodationPreferences_UserAccommodati~",
                        column: x => x.UserAccommodationPreferencesId,
                        principalTable: "UserAccommodationPreferences",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StandardRoutes_UserFoodPreferences_UserFoodPreferencesId",
                        column: x => x.UserFoodPreferencesId,
                        principalTable: "UserFoodPreferences",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StandardRoutes_UserPersonalizations_UserPersonalizationId",
                        column: x => x.UserPersonalizationId,
                        principalTable: "UserPersonalizations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DisplayNames",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: true),
                    LangugageCode = table.Column<string>(type: "text", nullable: true),
                    BaseRestaurantPlaceEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    BaseTravelPlaceEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisplayNames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DisplayNames_BaseRestaurantPlaceEntity_BaseRestaurantPlaceE~",
                        column: x => x.BaseRestaurantPlaceEntityId,
                        principalTable: "BaseRestaurantPlaceEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DisplayNames_BaseTravelPlaceEntity_BaseTravelPlaceEntityId",
                        column: x => x.BaseTravelPlaceEntityId,
                        principalTable: "BaseTravelPlaceEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    BaseRestaurantPlaceEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    BaseTravelPlaceEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Locations_BaseRestaurantPlaceEntity_BaseRestaurantPlaceEnti~",
                        column: x => x.BaseRestaurantPlaceEntityId,
                        principalTable: "BaseRestaurantPlaceEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Locations_BaseTravelPlaceEntity_BaseTravelPlaceEntityId",
                        column: x => x.BaseTravelPlaceEntityId,
                        principalTable: "BaseTravelPlaceEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AcceptsCreditCards = table.Column<string>(type: "text", nullable: true),
                    AcceptsDebitCards = table.Column<string>(type: "text", nullable: true),
                    AcceptsCashOnly = table.Column<string>(type: "text", nullable: true),
                    BaseRestaurantPlaceEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    BaseTravelPlaceEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentOptions_BaseRestaurantPlaceEntity_BaseRestaurantPlac~",
                        column: x => x.BaseRestaurantPlaceEntityId,
                        principalTable: "BaseRestaurantPlaceEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentOptions_BaseTravelPlaceEntity_BaseTravelPlaceEntityId",
                        column: x => x.BaseTravelPlaceEntityId,
                        principalTable: "BaseTravelPlaceEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    WidthPx = table.Column<int>(type: "integer", nullable: true),
                    HeightPx = table.Column<int>(type: "integer", nullable: true),
                    BaseRestaurantPlaceEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    BaseTravelPlaceEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Photos_BaseRestaurantPlaceEntity_BaseRestaurantPlaceEntityId",
                        column: x => x.BaseRestaurantPlaceEntityId,
                        principalTable: "BaseRestaurantPlaceEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Photos_BaseTravelPlaceEntity_BaseTravelPlaceEntityId",
                        column: x => x.BaseTravelPlaceEntityId,
                        principalTable: "BaseTravelPlaceEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegularOpeningHours",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OpenNow = table.Column<bool>(type: "boolean", nullable: true),
                    WeekdayDescriptions = table.Column<List<string>>(type: "text[]", nullable: true),
                    BaseRestaurantPlaceEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    BaseTravelPlaceEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegularOpeningHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegularOpeningHours_BaseRestaurantPlaceEntity_BaseRestauran~",
                        column: x => x.BaseRestaurantPlaceEntityId,
                        principalTable: "BaseRestaurantPlaceEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegularOpeningHours_BaseTravelPlaceEntity_BaseTravelPlaceEn~",
                        column: x => x.BaseTravelPlaceEntityId,
                        principalTable: "BaseTravelPlaceEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TravelDays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DayDescription = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    ApproxPrice = table.Column<string>(type: "text", nullable: true),
                    StandardRouteId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TravelDays_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TravelDays_StandardRoutes_StandardRouteId",
                        column: x => x.StandardRouteId,
                        principalTable: "StandardRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLikedRoutes",
                columns: table => new
                {
                    LikedRoutesId = table.Column<Guid>(type: "uuid", nullable: false),
                    LikedUsersId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLikedRoutes", x => new { x.LikedRoutesId, x.LikedUsersId });
                    table.ForeignKey(
                        name: "FK_UserLikedRoutes_AspNetUsers_LikedUsersId",
                        column: x => x.LikedUsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserLikedRoutes_StandardRoutes_LikedRoutesId",
                        column: x => x.LikedRoutesId,
                        principalTable: "StandardRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Periods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RegularOpeningHoursId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Periods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Periods_RegularOpeningHours_RegularOpeningHoursId",
                        column: x => x.RegularOpeningHoursId,
                        principalTable: "RegularOpeningHours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Breakfasts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TravelDayId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Breakfasts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Breakfasts_BaseRestaurantPlaceEntity_Id",
                        column: x => x.Id,
                        principalTable: "BaseRestaurantPlaceEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Breakfasts_TravelDays_TravelDayId",
                        column: x => x.TravelDayId,
                        principalTable: "TravelDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dinners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServesBeer = table.Column<bool>(type: "boolean", nullable: true),
                    ServesWine = table.Column<bool>(type: "boolean", nullable: true),
                    TravelDayId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dinners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dinners_BaseRestaurantPlaceEntity_Id",
                        column: x => x.Id,
                        principalTable: "BaseRestaurantPlaceEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dinners_TravelDays_TravelDayId",
                        column: x => x.TravelDayId,
                        principalTable: "TravelDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lunches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServesBeer = table.Column<bool>(type: "boolean", nullable: true),
                    ServesWine = table.Column<bool>(type: "boolean", nullable: true),
                    TravelDayId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lunches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lunches_BaseRestaurantPlaceEntity_Id",
                        column: x => x.Id,
                        principalTable: "BaseRestaurantPlaceEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Lunches_TravelDays_TravelDayId",
                        column: x => x.TravelDayId,
                        principalTable: "TravelDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Places",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WeatherId = table.Column<Guid>(type: "uuid", nullable: true),
                    FirstPlaceTravelDayId = table.Column<Guid>(type: "uuid", nullable: true),
                    SecondPlaceTravelDayId = table.Column<Guid>(type: "uuid", nullable: true),
                    ThirdPlaceTravelDayId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Places", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Places_BaseTravelPlaceEntity_Id",
                        column: x => x.Id,
                        principalTable: "BaseTravelPlaceEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Places_TravelDays_FirstPlaceTravelDayId",
                        column: x => x.FirstPlaceTravelDayId,
                        principalTable: "TravelDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Places_TravelDays_SecondPlaceTravelDayId",
                        column: x => x.SecondPlaceTravelDayId,
                        principalTable: "TravelDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Places_TravelDays_ThirdPlaceTravelDayId",
                        column: x => x.ThirdPlaceTravelDayId,
                        principalTable: "TravelDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Places_Weathers_WeatherId",
                        column: x => x.WeatherId,
                        principalTable: "Weathers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PlacesAfterDinner",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Takeout = table.Column<bool>(type: "boolean", nullable: true),
                    Delivery = table.Column<bool>(type: "boolean", nullable: true),
                    CurbsidePickup = table.Column<bool>(type: "boolean", nullable: true),
                    ServesBeer = table.Column<bool>(type: "boolean", nullable: true),
                    ServesWine = table.Column<bool>(type: "boolean", nullable: true),
                    ServesCocktails = table.Column<bool>(type: "boolean", nullable: true),
                    GoodForChildren = table.Column<bool>(type: "boolean", nullable: true),
                    TravelDayId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlacesAfterDinner", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlacesAfterDinner_BaseRestaurantPlaceEntity_Id",
                        column: x => x.Id,
                        principalTable: "BaseRestaurantPlaceEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlacesAfterDinner_TravelDays_TravelDayId",
                        column: x => x.TravelDayId,
                        principalTable: "TravelDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TravelAccomodations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Star = table.Column<string>(type: "text", nullable: true),
                    InternationalPhoneNumber = table.Column<string>(type: "text", nullable: true),
                    UserAccomodationPreference = table.Column<string>(type: "text", nullable: true),
                    TravelDayId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelAccomodations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TravelAccomodations_BaseTravelPlaceEntity_Id",
                        column: x => x.Id,
                        principalTable: "BaseTravelPlaceEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TravelAccomodations_TravelDays_TravelDayId",
                        column: x => x.TravelDayId,
                        principalTable: "TravelDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Closes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Day = table.Column<int>(type: "integer", nullable: true),
                    Hour = table.Column<int>(type: "integer", nullable: true),
                    Minute = table.Column<int>(type: "integer", nullable: true),
                    PeriodId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Closes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Closes_Periods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "Periods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Opens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Day = table.Column<int>(type: "integer", nullable: true),
                    Hour = table.Column<int>(type: "integer", nullable: true),
                    Minute = table.Column<int>(type: "integer", nullable: true),
                    PeriodId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Opens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Opens_Periods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "Periods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AllWorldCities_city",
                table: "AllWorldCities",
                column: "city");

            migrationBuilder.CreateIndex(
                name: "IX_AllWorldCities_country",
                table: "AllWorldCities",
                column: "country");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_BaseRestaurantPlaceEntity_WeatherId",
                table: "BaseRestaurantPlaceEntity",
                column: "WeatherId");

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
                name: "IX_Breakfasts_TravelDayId",
                table: "Breakfasts",
                column: "TravelDayId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Closes_PeriodId",
                table: "Closes",
                column: "PeriodId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_AlphabeticCode",
                table: "Currencies",
                column: "AlphabeticCode");

            migrationBuilder.CreateIndex(
                name: "IX_Dinners_TravelDayId",
                table: "Dinners",
                column: "TravelDayId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DisplayNames_BaseRestaurantPlaceEntityId",
                table: "DisplayNames",
                column: "BaseRestaurantPlaceEntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DisplayNames_BaseTravelPlaceEntityId",
                table: "DisplayNames",
                column: "BaseTravelPlaceEntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_BaseRestaurantPlaceEntityId",
                table: "Locations",
                column: "BaseRestaurantPlaceEntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_BaseTravelPlaceEntityId",
                table: "Locations",
                column: "BaseTravelPlaceEntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lunches_TravelDayId",
                table: "Lunches",
                column: "TravelDayId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Opens_PeriodId",
                table: "Opens",
                column: "PeriodId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOptions_BaseRestaurantPlaceEntityId",
                table: "PaymentOptions",
                column: "BaseRestaurantPlaceEntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOptions_BaseTravelPlaceEntityId",
                table: "PaymentOptions",
                column: "BaseTravelPlaceEntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Periods_RegularOpeningHoursId",
                table: "Periods",
                column: "RegularOpeningHoursId");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_BaseRestaurantPlaceEntityId",
                table: "Photos",
                column: "BaseRestaurantPlaceEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_BaseTravelPlaceEntityId",
                table: "Photos",
                column: "BaseTravelPlaceEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Places_FirstPlaceTravelDayId",
                table: "Places",
                column: "FirstPlaceTravelDayId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Places_SecondPlaceTravelDayId",
                table: "Places",
                column: "SecondPlaceTravelDayId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Places_ThirdPlaceTravelDayId",
                table: "Places",
                column: "ThirdPlaceTravelDayId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Places_WeatherId",
                table: "Places",
                column: "WeatherId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacesAfterDinner_TravelDayId",
                table: "PlacesAfterDinner",
                column: "TravelDayId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegularOpeningHours_BaseRestaurantPlaceEntityId",
                table: "RegularOpeningHours",
                column: "BaseRestaurantPlaceEntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegularOpeningHours_BaseTravelPlaceEntityId",
                table: "RegularOpeningHours",
                column: "BaseTravelPlaceEntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StandardRoutes_Status",
                table: "StandardRoutes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_StandardRoutes_UserAccommodationPreferencesId",
                table: "StandardRoutes",
                column: "UserAccommodationPreferencesId");

            migrationBuilder.CreateIndex(
                name: "IX_StandardRoutes_UserFoodPreferencesId",
                table: "StandardRoutes",
                column: "UserFoodPreferencesId");

            migrationBuilder.CreateIndex(
                name: "IX_StandardRoutes_UserId",
                table: "StandardRoutes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StandardRoutes_UserPersonalizationId",
                table: "StandardRoutes",
                column: "UserPersonalizationId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelAccomodations_TravelDayId",
                table: "TravelAccomodations",
                column: "TravelDayId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TravelDays_StandardRouteId",
                table: "TravelDays",
                column: "StandardRouteId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelDays_UserId",
                table: "TravelDays",
                column: "UserId");

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
                name: "IX_UserFriendships_AddresseeId_Status",
                table: "UserFriendships",
                columns: new[] { "AddresseeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_UserFriendships_RequesterId_AddresseeId",
                table: "UserFriendships",
                columns: new[] { "RequesterId", "AddresseeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserFriendships_RequesterId_Status",
                table: "UserFriendships",
                columns: new[] { "RequesterId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_UserLikedRoutes_LikedUsersId",
                table: "UserLikedRoutes",
                column: "LikedUsersId");

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
                name: "Breakfasts");

            migrationBuilder.DropTable(
                name: "Closes");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "Dinners");

            migrationBuilder.DropTable(
                name: "DisplayNames");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "Lunches");

            migrationBuilder.DropTable(
                name: "Opens");

            migrationBuilder.DropTable(
                name: "PaymentOptions");

            migrationBuilder.DropTable(
                name: "Photos");

            migrationBuilder.DropTable(
                name: "Places");

            migrationBuilder.DropTable(
                name: "PlacesAfterDinner");

            migrationBuilder.DropTable(
                name: "TravelAccomodations");

            migrationBuilder.DropTable(
                name: "UserFriendships");

            migrationBuilder.DropTable(
                name: "UserLikedRoutes");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Periods");

            migrationBuilder.DropTable(
                name: "TravelDays");

            migrationBuilder.DropTable(
                name: "RegularOpeningHours");

            migrationBuilder.DropTable(
                name: "StandardRoutes");

            migrationBuilder.DropTable(
                name: "BaseRestaurantPlaceEntity");

            migrationBuilder.DropTable(
                name: "BaseTravelPlaceEntity");

            migrationBuilder.DropTable(
                name: "Weathers");

            migrationBuilder.DropTable(
                name: "UserAccommodationPreferences");

            migrationBuilder.DropTable(
                name: "UserFoodPreferences");

            migrationBuilder.DropTable(
                name: "UserPersonalizations");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
