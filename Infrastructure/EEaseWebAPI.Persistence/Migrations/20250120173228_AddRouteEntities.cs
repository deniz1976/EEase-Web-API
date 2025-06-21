using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EEaseWebAPI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRouteEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Days",
                table: "StandardRoutes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LikeCount",
                table: "StandardRoutes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "StandardRoutes",
                type: "text",
                nullable: true);

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
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseTravelPlaceEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TravelDays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DayDescription = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: true),
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
                name: "TravelAccomodations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Star = table.Column<string>(type: "text", nullable: true),
                    InternationalPhoneNumber = table.Column<string>(type: "text", nullable: true),
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
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseRestaurantPlaceEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaseRestaurantPlaceEntity_Weathers_WeatherId",
                        column: x => x.WeatherId,
                        principalTable: "Weathers",
                        principalColumn: "Id");
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
                name: "IX_StandardRoutes_UserId",
                table: "StandardRoutes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseRestaurantPlaceEntity_WeatherId",
                table: "BaseRestaurantPlaceEntity",
                column: "WeatherId");

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
                name: "IX_UserLikedRoutes_LikedUsersId",
                table: "UserLikedRoutes",
                column: "LikedUsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_StandardRoutes_AspNetUsers_UserId",
                table: "StandardRoutes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StandardRoutes_AspNetUsers_UserId",
                table: "StandardRoutes");

            migrationBuilder.DropTable(
                name: "Breakfasts");

            migrationBuilder.DropTable(
                name: "Closes");

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
                name: "UserLikedRoutes");

            migrationBuilder.DropTable(
                name: "Periods");

            migrationBuilder.DropTable(
                name: "TravelDays");

            migrationBuilder.DropTable(
                name: "RegularOpeningHours");

            migrationBuilder.DropTable(
                name: "BaseRestaurantPlaceEntity");

            migrationBuilder.DropTable(
                name: "BaseTravelPlaceEntity");

            migrationBuilder.DropTable(
                name: "Weathers");

            migrationBuilder.DropIndex(
                name: "IX_StandardRoutes_UserId",
                table: "StandardRoutes");

            migrationBuilder.DropColumn(
                name: "Days",
                table: "StandardRoutes");

            migrationBuilder.DropColumn(
                name: "LikeCount",
                table: "StandardRoutes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "StandardRoutes");
        }
    }
}
