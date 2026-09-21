using EEaseWebAPI.Domain.Entities.Route;

namespace EEaseWebAPI.Persistence.Services.Route
{
    internal static class RouteBuilding
    {
        public const int TouristicPlacesPerDay = 3;
        public const int PublishedRouteStatus = 2;

        public static void ValidateParameters(string? destination, DateOnly? startDate, DateOnly? endDate)
        {
            if (string.IsNullOrWhiteSpace(destination))
                throw new ArgumentNullException(nameof(destination));

            if (startDate == null)
                throw new ArgumentNullException(nameof(startDate));

            if (endDate == null)
                throw new ArgumentNullException(nameof(endDate));

            if (endDate < startDate)
                throw new ArgumentException("The end date cannot be before the start date.", nameof(endDate));
        }

        public static string FormatDestination(string? destination)
        {
            var trimmed = destination!.Trim();

            return char.ToUpperInvariant(trimmed[0]) + trimmed[1..].ToLowerInvariant();
        }

        public static int CalculateDayCount(DateOnly? startDate, DateOnly? endDate) =>
            (endDate!.Value.DayNumber - startDate!.Value.DayNumber) + 1;

        public static StandardRoute InitializeStandardRoute(string destination, int dayCount, string ownerId)
        {
            var standardRoute = new StandardRoute
            {
                City = destination,
                User = null,
                LikedUsers = new List<Domain.Entities.Identity.AppUser>(),
                Name = destination,
                UserId = ownerId,
                Days = dayCount,
                Id = Guid.NewGuid(),
                LikeCount = 0,
                TravelDays = new List<TravelDay>(),
                Status = PublishedRouteStatus
            };

            for (int i = 0; i < dayCount; i++)
            {
                standardRoute.TravelDays.Add(new TravelDay());
            }

            return standardRoute;
        }

        public static TravelAccomodation CopyAccommodation(TravelAccomodation source, PRICE_LEVEL? priceLevel) =>
            new()
            {
                Id = Guid.NewGuid(),
                _PRICE_LEVEL = priceLevel,
                DisplayName = source.DisplayName,
                FormattedAddress = source.FormattedAddress,
                GoogleId = source.GoogleId,
                GoogleMapsUri = source.GoogleMapsUri,
                Location = source.Location,
                NationalPhoneNumber = source.NationalPhoneNumber,
                Photos = source.Photos,
                PrimaryType = source.PrimaryType,
                Rating = source.Rating,
                RegularOpeningHours = source.RegularOpeningHours,
                Restroom = source.Restroom,
                WebsiteUri = source.WebsiteUri,
                UserAccomodationPreference = source.UserAccomodationPreference
            };
    }
}
