using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Common;
using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.Route;
using EEaseWebAPI.Domain.Entities.Common;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace EEaseWebAPI.Persistence.Services.Route
{
    public sealed class PreferenceFeedbackService : IPreferenceFeedbackService
    {
        private readonly EEaseAPIDbContext _context;
        private readonly IGeminiAIService _geminiAIService;

        public PreferenceFeedbackService(EEaseAPIDbContext context, IGeminiAIService geminiAIService)
        {
            _context = context;
            _geminiAIService = geminiAIService;
        }

        public async Task<PreferenceFeedbackResult> ApplyAsync(
            string userId, BaseEntity place, string placeType, bool liked)
        {
            var category = CategoryOf(placeType);

            var preferences = await LoadAsync(category, userId)
                ?? throw new BaseException(
                    "User personalization not found, you must introduce yourself.",
                    (int)StatusEnum.PreferenceDescriptionsRetrievalFailed);

            var (name, type, description) = Describe(place, placeType, category);

            var available = preferences.GetType()
                .GetProperties()
                .Where(IsPreferenceProperty)
                .Select(property => property.Name)
                .ToList();

            var selected = await _geminiAIService.AnalyzePlacePreferencesAsync(name, type, description, available);

            if (selected == null || selected.Count == 0)
            {
                return PreferenceFeedbackResult.None;
            }

            var changes = new Dictionary<string, int>();

            foreach (var preferenceName in selected)
            {
                var property = preferences.GetType().GetProperty(preferenceName);

                if (property == null || !IsPreferenceProperty(property))
                {
                    continue;
                }

                var current = (int?)property.GetValue(preferences);
                var updated = liked ? PreferenceScoring.Reinforce(current) : PreferenceScoring.Weaken(current);

                property.SetValue(preferences, updated);
                changes[preferenceName] = updated - (current ?? PreferenceScoring.MinScore);
            }

            await _context.SaveChangesAsync();

            return new PreferenceFeedbackResult(category, changes);
        }

        private static string CategoryOf(string placeType) => placeType.ToLowerInvariant() switch
        {
            "accommodation" => "accommodation",
            "breakfast" or "lunch" or "dinner" or "placeafterdinner" => "food",
            "firstplace" or "secondplace" or "thirdplace" or "place" => "personalization",
            _ => throw new InvalidPlaceTypeException($"Invalid place type: {placeType}")
        };

        private Task<object?> LoadAsync(string category, string userId) => category switch
        {
            "accommodation" => Cast(_context.UserAccommodationPreferences
                .FirstOrDefaultAsync(preference => preference.UserId == userId)),
            "food" => Cast(_context.UserFoodPreferences
                .FirstOrDefaultAsync(preference => preference.UserId == userId)),
            _ => Cast(_context.UserPersonalizations
                .FirstOrDefaultAsync(preference => preference.UserId == userId))
        };

        private static async Task<object?> Cast<T>(Task<T?> task) where T : class => await task;

        private static (string Name, string Type, string Description) Describe(
            BaseEntity place, string placeType, string category)
        {
            switch (category)
            {
                case "accommodation":
                    var accommodation = place as TravelAccomodation
                        ?? throw new InvalidPlaceTypeException("Invalid accommodation type");

                    return (
                        accommodation.DisplayName?.Text ?? "Unknown Accommodation",
                        "Accommodation",
                        $"Name: {accommodation.DisplayName?.Text}, " +
                        $"Type: {accommodation.PrimaryType}, " +
                        $"Rating: {accommodation.Rating}, " +
                        $"Price Level: {accommodation._PRICE_LEVEL}");

                case "food":
                    var restaurant = place as BaseRestaurantPlaceEntity
                        ?? throw new InvalidPlaceTypeException("Invalid restaurant type");

                    return (
                        restaurant.DisplayName?.Text ?? $"Unknown {placeType}",
                        placeType,
                        $"Name: {restaurant.DisplayName?.Text}, " +
                        $"Type: {restaurant.PrimaryType}, " +
                        $"Rating: {restaurant.Rating}, " +
                        $"Price Level: {restaurant._PRICE_LEVEL}, " +
                        $"Meal Type: {placeType}");

                default:
                    var travelPlace = place as BaseTravelPlaceEntity
                        ?? throw new InvalidPlaceTypeException("Invalid place type");

                    return (
                        travelPlace.DisplayName?.Text ?? "Unknown Place",
                        travelPlace.PrimaryType ?? "Unknown Type",
                        $"Name: {travelPlace.DisplayName?.Text}, " +
                        $"Type: {travelPlace.PrimaryType}, " +
                        $"Rating: {travelPlace.Rating}");
            }
        }

        private static bool IsPreferenceProperty(PropertyInfo property) =>
            property.PropertyType == typeof(int?) &&
            property.Name.EndsWith("Preference", StringComparison.Ordinal);
    }
}
