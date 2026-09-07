using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Domain.Enums;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IPlaceQueryBuilder
    {
        string HotelStars(PRICE_LEVEL? priceLevel);

        string PricePrefix(PRICE_LEVEL? priceLevel);

        string Accommodation(IReadOnlyList<PreferenceItem>? preferences, PRICE_LEVEL? priceLevel);

        string Food(IReadOnlyList<PreferenceItem>? preferences, PRICE_LEVEL? priceLevel, MealType mealType);

        string Touristic(IReadOnlyList<PreferenceItem>? preferences);

        string AfterDinner(IReadOnlyList<PreferenceItem>? preferences, PRICE_LEVEL? priceLevel);

        string AlternativeTouristic(IReadOnlyList<PreferenceItem>? preferences, string primaryQuery);

        string AlternativeAfterDinner(PRICE_LEVEL? priceLevel);

        string AlternativeFood(MealType mealType, PRICE_LEVEL? priceLevel);

        string SelectPreference(IReadOnlyList<PreferenceItem>? preferences);
    }
}
