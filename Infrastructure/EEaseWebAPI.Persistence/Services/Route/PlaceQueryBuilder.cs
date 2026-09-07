using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Domain.Enums;

namespace EEaseWebAPI.Persistence.Services.Route
{
    public sealed class PlaceQueryBuilder : IPlaceQueryBuilder
    {
        private const int DominantThreshold = 60;
        private const int DominantWinChance = 80;
        private const double SelectionSharpness = 2d;

        private static readonly string[] AfterDinnerFallbacks =
        {
            "Live music bars",
            "Modern rooftop bars",
            "Famous cocktail bars",
            "Special dessert shops",
            "Trendy nightclubs"
        };

        private static readonly Dictionary<string, string> AccommodationKeywords = new()
        {
            ["LuxuryHotelPreference"] = " luxury",
            ["BudgetHotelPreference"] = " budget",
            ["BoutiqueHotelPreference"] = " boutique",
            ["HostelPreference"] = " hostel",
            ["ApartmentPreference"] = " apartment",
            ["ResortPreference"] = " resort",
            ["VillaPreference"] = " villa",
            ["GuestHousePreference"] = " guest house",
            ["CampingPreference"] = " camping",
            ["GlampingPreference"] = " glamping",
            ["BedAndBreakfastPreference"] = " bed and breakfast",
            ["AllInclusivePreference"] = " all inclusive",
            ["SpaAndWellnessPreference"] = " with spa",
            ["PetFriendlyPreference"] = " pet friendly",
            ["EcoFriendlyPreference"] = " eco friendly",
            ["RemoteLocationPreference"] = " remote location",
            ["CityCenterPreference"] = " city center",
            ["FamilyFriendlyPreference"] = " family friendly",
            ["AdultsOnlyPreference"] = " adults only",
            ["HomestayPreference"] = " homestay",
            ["WaterfrontPreference"] = " waterfront",
            ["HistoricalBuildingPreference"] = " historical building",
            ["AirbnbPreference"] = " airbnb style",
            ["CoLivingSpacePreference"] = " co-living space",
            ["ExtendedStayPreference"] = " extended stay"
        };

        private static readonly Dictionary<string, string> FoodKeywords = new()
        {
            ["VeganPreference"] = "vegan ",
            ["VegetarianPreference"] = "vegetarian ",
            ["GlutenFreePreference"] = "gluten free ",
            ["HalalPreference"] = "halal ",
            ["KosherPreference"] = "kosher ",
            ["SeafoodPreference"] = "seafood ",
            ["LocalCuisinePreference"] = "local cuisine ",
            ["FastFoodPreference"] = "fast food ",
            ["FinePreference"] = "fine dining ",
            ["StreetFoodPreference"] = "street food ",
            ["OrganicPreference"] = "organic ",
            ["BuffetPreference"] = "buffet ",
            ["FoodTruckPreference"] = "food truck ",
            ["CafeteriaPreference"] = "cafeteria ",
            ["DeliveryPreference"] = "delivery ",
            ["AllergiesPreference"] = "allergy friendly ",
            ["DairyFreePreference"] = "dairy free ",
            ["NutFreePreference"] = "nut free ",
            ["SpicyPreference"] = "spicy ",
            ["SweetPreference"] = "sweet ",
            ["SaltyPreference"] = "salty ",
            ["SourPreference"] = "sour ",
            ["BitterPreference"] = "bitter ",
            ["UmamiPreference"] = "umami ",
            ["FusionPreference"] = "fusion "
        };

        private static readonly Dictionary<string, string> TouristicQueries = new()
        {
            ["AdventurePreference"] = "Adventure activities",
            ["RelaxationPreference"] = "Relaxation spots",
            ["CulturalPreference"] = "Cultural attractions",
            ["NaturePreference"] = "Breathtaking natural attractions",
            ["UrbanPreference"] = "Urban attractions",
            ["RuralPreference"] = "Rural attractions",
            ["LuxuryPreference"] = "Exclusive attractions",
            ["BudgetPreference"] = "Free tourist attractions",
            ["SoloTravelPreference"] = "Solo traveler spots",
            ["GroupTravelPreference"] = "Group friendly attractions",
            ["FamilyTravelPreference"] = "Family-friendly attractions",
            ["CoupleTravelPreference"] = "Romantic spots",
            ["BeachPreference"] = "Best beaches",
            ["MountainPreference"] = "Mountain attractions",
            ["DesertPreference"] = "Desert attractions",
            ["ForestPreference"] = "Forest attractions",
            ["IslandPreference"] = "Island attractions",
            ["LakePreference"] = "Lake attractions",
            ["RiverPreference"] = "River attractions",
            ["WaterfallPreference"] = "Waterfall attractions",
            ["CavePreference"] = "Cave attractions",
            ["VolcanoPreference"] = "Volcano attractions",
            ["GlacierPreference"] = "Glacier attractions",
            ["CanyonPreference"] = "Canyon attractions",
            ["ValleyPreference"] = "Valley attractions"
        };

        private static readonly Dictionary<string, string> AfterDinnerQueries = new()
        {
            ["AdventurePreference"] = "Adventure activities",
            ["RelaxationPreference"] = "Quiet lounge bars",
            ["CulturalPreference"] = "Cultural evening activities",
            ["NaturePreference"] = "Nature viewpoints",
            ["UrbanPreference"] = "Urban nightlife",
            ["RuralPreference"] = "Rural evening entertainment",
            ["LuxuryPreference"] = "Exclusive rooftop bars",
            ["BudgetPreference"] = "Budget friendly evening spots",
            ["SoloTravelPreference"] = "Solo traveler friendly bars",
            ["GroupTravelPreference"] = "Group friendly evening venues",
            ["FamilyTravelPreference"] = "Family-friendly evening activities",
            ["CoupleTravelPreference"] = "Romantic evening spots",
            ["BeachPreference"] = "Beach bars",
            ["MountainPreference"] = "Mountain viewpoint bars",
            ["IslandPreference"] = "Island nightlife"
        };

        private static readonly Dictionary<string, string> AlternativeTouristicQueries = new()
        {
            ["AdventurePreference"] = "Adventure spots",
            ["RelaxationPreference"] = "Relaxation sites",
            ["CulturalPreference"] = "Cultural sites",
            ["NaturePreference"] = "Natural attractions",
            ["UrbanPreference"] = "Urban attractions",
            ["RuralPreference"] = "Rural attractions",
            ["LuxuryPreference"] = "Luxury attractions",
            ["BudgetPreference"] = "Budget attractions",
            ["SoloTravelPreference"] = "Solo traveler spots",
            ["GroupTravelPreference"] = "Group attractions",
            ["FamilyTravelPreference"] = "Family attractions",
            ["CoupleTravelPreference"] = "Romantic sites",
            ["BeachPreference"] = "Beach attractions",
            ["MountainPreference"] = "Mountain attractions",
            ["DesertPreference"] = "Desert attractions",
            ["ForestPreference"] = "Forest attractions",
            ["IslandPreference"] = "Island attractions",
            ["LakePreference"] = "Lake attractions",
            ["RiverPreference"] = "River attractions",
            ["WaterfallPreference"] = "Waterfall sites",
            ["CavePreference"] = "Cave attractions",
            ["VolcanoPreference"] = "Volcano sites",
            ["GlacierPreference"] = "Glacier attractions",
            ["CanyonPreference"] = "Canyon attractions",
            ["ValleyPreference"] = "Valley attractions"
        };

        private static readonly string[] AlternativeTouristicFallbacks =
        {
            "Must visit spots",
            "Popular tourist destinations",
            "Top-rated places to visit",
            "Interesting places to see"
        };

        private static readonly string[] AlternativeAfterDinnerQueries =
        {
            "Quiet cafes",
            "Jazz bars",
            "Karaoke places",
            "Comedy clubs",
            "Dessert places",
            "Late night food",
            "Evening entertainment",
            "Classy lounges"
        };

        private static readonly Dictionary<MealType, string[]> AlternativeFoodQueries = new()
        {
            [MealType.Breakfast] = new[] { "Cafe", "Coffee shop", "Bakery", "Brunch", "Morning restaurant" },
            [MealType.Lunch] = new[] { "Bistro", "Casual restaurant", "Deli", "Sandwich shop", "Quick lunch" },
            [MealType.Dinner] = new[] { "Fine dining", "Restaurant", "Steakhouse", "Grill", "Evening dining" }
        };

        private readonly Random _random;

        public PlaceQueryBuilder()
            : this(Random.Shared)
        {
        }

        public PlaceQueryBuilder(Random random)
        {
            _random = random;
        }

        public string HotelStars(PRICE_LEVEL? priceLevel) => priceLevel switch
        {
            PRICE_LEVEL.PRICE_LEVEL_INEXPENSIVE => "3",
            PRICE_LEVEL.PRICE_LEVEL_MODERATE => "4",
            PRICE_LEVEL.PRICE_LEVEL_EXPENSIVE => "5",
            PRICE_LEVEL.PRICE_LEVEL_VERY_EXPENSIVE => "5",
            _ => "4"
        };

        public string PricePrefix(PRICE_LEVEL? priceLevel) => priceLevel switch
        {
            PRICE_LEVEL.PRICE_LEVEL_INEXPENSIVE => "Inexpensive ",
            PRICE_LEVEL.PRICE_LEVEL_MODERATE => "Moderate ",
            PRICE_LEVEL.PRICE_LEVEL_EXPENSIVE => "Expensive ",
            PRICE_LEVEL.PRICE_LEVEL_VERY_EXPENSIVE => "Very expensive ",
            _ => "Moderate "
        };

        public string Accommodation(IReadOnlyList<PreferenceItem>? preferences, PRICE_LEVEL? priceLevel)
        {
            var query = $"{HotelStars(priceLevel)} star hotel";

            if (preferences == null || preferences.Count == 0)
            {
                return query;
            }

            var selected = SelectPreference(preferences);

            return AccommodationKeywords.TryGetValue(selected, out var keyword)
                ? query + keyword
                : query;
        }

        public string Food(IReadOnlyList<PreferenceItem>? preferences, PRICE_LEVEL? priceLevel, MealType mealType)
        {
            var mealPrefix = mealType switch
            {
                MealType.Breakfast => "Breakfast restaurant ",
                MealType.Lunch => "Lunch restaurant ",
                MealType.Dinner => "Dinner restaurant ",
                _ => "Restaurant "
            };

            var baseQuery = $"{PricePrefix(priceLevel)}{mealPrefix}";

            if (preferences == null || preferences.Count == 0)
            {
                return baseQuery.Trim();
            }

            var selected = SelectPreference(preferences);

            return FoodKeywords.TryGetValue(selected, out var keyword)
                ? (baseQuery + keyword).Trim()
                : baseQuery.Trim();
        }

        public string Touristic(IReadOnlyList<PreferenceItem>? preferences)
        {
            if (preferences == null || preferences.Count < 2)
            {
                return "Tourist attractions";
            }

            var selected = SelectPreference(preferences);

            return TouristicQueries.TryGetValue(selected, out var query)
                ? query
                : "Famous tourist attractions";
        }

        public string AfterDinner(IReadOnlyList<PreferenceItem>? preferences, PRICE_LEVEL? priceLevel)
        {
            var pricePrefix = PricePrefix(priceLevel);

            if (preferences == null || preferences.Count == 0)
            {
                return pricePrefix + Pick(AfterDinnerFallbacks);
            }

            var selected = SelectPreference(preferences);

            return AfterDinnerQueries.TryGetValue(selected, out var query)
                ? pricePrefix + query
                : pricePrefix + Pick(AfterDinnerFallbacks);
        }

        public string AlternativeTouristic(IReadOnlyList<PreferenceItem>? preferences, string primaryQuery)
        {
            if (preferences == null || preferences.Count < 2)
            {
                return Pick(AlternativeTouristicFallbacks);
            }

            var unused = preferences
                .Where(preference => !primaryQuery.Contains(Keyword(preference.Name), StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (unused.Count == 0)
            {
                return Pick(AlternativeTouristicFallbacks);
            }

            return AlternativeTouristicQueries.TryGetValue(SelectPreference(unused), out var query)
                ? query
                : Pick(AlternativeTouristicFallbacks);
        }

        public string AlternativeAfterDinner(PRICE_LEVEL? priceLevel) =>
            PricePrefix(priceLevel) + Pick(AlternativeAfterDinnerQueries);

        public string AlternativeFood(MealType mealType, PRICE_LEVEL? priceLevel)
        {
            var prefix = PricePrefix(priceLevel);

            return AlternativeFoodQueries.TryGetValue(mealType, out var queries)
                ? prefix + Pick(queries)
                : prefix + "Restaurant";
        }

        public string SelectPreference(IReadOnlyList<PreferenceItem>? preferences)
        {
            if (preferences == null || preferences.Count == 0)
            {
                return string.Empty;
            }

            var mandatory = preferences.Where(preference => preference.IsMandatory).ToList();

            if (mandatory.Count > 0)
            {
                return mandatory
                    .OrderByDescending(preference => preference.Priority)
                    .ThenByDescending(preference => preference.Score)
                    .First()
                    .Name;
            }

            var dominant = preferences
                .Where(preference => preference.IsDominant && preference.Score > DominantThreshold)
                .ToList();

            if (dominant.Count > 0 && _random.Next(100) < DominantWinChance)
            {
                return WeightedPick(dominant);
            }

            return WeightedPick(preferences);
        }

        private string Pick(string[] options) => options[_random.Next(options.Length)];

        private static string Keyword(string preferenceName) =>
            preferenceName.EndsWith("Preference", StringComparison.Ordinal)
                ? preferenceName[..^"Preference".Length]
                : preferenceName;

        private string WeightedPick(IReadOnlyList<PreferenceItem> preferences)
        {
            var weights = preferences
                .Select(preference => Math.Pow(Math.Max(preference.Weight, 0d), SelectionSharpness))
                .ToArray();

            var total = weights.Sum();

            if (total <= 0d)
            {
                return preferences[_random.Next(preferences.Count)].Name;
            }

            var target = _random.NextDouble() * total;
            var cumulative = 0d;

            for (var index = 0; index < preferences.Count; index++)
            {
                cumulative += weights[index];

                if (target < cumulative)
                {
                    return preferences[index].Name;
                }
            }

            return preferences[^1].Name;
        }
    }
}
