using System.Collections.Generic;
using System.Linq;

namespace EEaseWebAPI.Application.MapEntities.PreferenceGroups
{
    public static class TravelPreferenceGroups
    {
        private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> AllGroups =
            new[] { AccommodationGroups.Groups, FoodGroups.Groups, TravelGroups.Groups }
                .SelectMany(group => group)
                .ToDictionary(entry => entry.Key, entry => entry.Value);

        public static IEnumerable<KeyValuePair<string, IReadOnlyList<string>>> All => AllGroups;

        public static bool TryGetPreferenceNames(string topic, out IReadOnlyList<string> preferenceNames) =>
            AllGroups.TryGetValue(topic, out preferenceNames!);

        public static class AccommodationGroups
        {
            public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> Groups =
                new Dictionary<string, IReadOnlyList<string>>
            {
                {
                    "Luxury Stays", new List<string>
                    {
                        "LuxuryHotelPreference",
                        "VillaPreference",
                        "ResortPreference"
                    }
                },
                {
                    "Budget-Friendly Stays", new List<string>
                    {
                        "BudgetHotelPreference",
                        "HostelPreference",
                        "GuestHousePreference"
                    }
                },
                {
                    "Camping & Glamping", new List<string>
                    {
                        "CampingPreference",
                        "GlampingPreference"
                    }
                },
                {
                    "Waterfront Getaways", new List<string>
                    {
                        "WaterfrontPreference",
                        "BeachPreference"
                    }
                },
                {
                    "Eco-Friendly Stays", new List<string>
                    {
                        "EcoFriendlyPreference"
                    }
                },
                {
                    "Pet-Friendly Options", new List<string>
                    {
                        "PetFriendlyPreference"
                    }
                },
                {
                    "Historical Places", new List<string>
                    {
                        "HistoricalBuildingPreference"
                    }
                },
                {
                    "City-Center Stays", new List<string>
                    {
                        "CityCenterPreference",
                        "UrbanPreference"
                    }
                },
                {
                    "Remote Retreats", new List<string>
                    {
                        "RemoteLocationPreference",
                        "RuralPreference"
                    }
                },
                {
                    "Family-Friendly Stays", new List<string>
                    {
                        "FamilyFriendlyPreference"
                    }
                },
                {
                    "Wellness & Spa", new List<string>
                    {
                        "SpaAndWellnessPreference"
                    }
                },
                {
                    "Extended Stay Options", new List<string>
                    {
                        "ExtendedStayPreference",
                        "CoLivingSpacePreference"
                    }
                },
                {
                    "Boutique & Unique", new List<string>
                    {
                        "BoutiqueHotelPreference",
                        "BedAndBreakfastPreference"
                    }
                },
                {
                    "Apartment Style", new List<string>
                    {
                        "ApartmentPreference",
                        "AirbnbPreference"
                    }
                },
                {
                    "Adults-Only Resorts", new List<string>
                    {
                        "AdultsOnlyPreference"
                    }
                },
                {
                    "All-Inclusive Options", new List<string>
                    {
                        "AllInclusivePreference"
                    }
                },
                {
                    "Local Living", new List<string>
                    {
                        "HomestayPreference"
                    }
                }
            };
        }

        public static class FoodGroups
        {
            public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> Groups =
                new Dictionary<string, IReadOnlyList<string>>
            {
                {
                    "Vegan & Vegetarian Options", new List<string>
                    {
                        "VegetarianPreference",
                        "VeganPreference"
                    }
                },
                {
                    "Local Cuisine", new List<string>
                    {
                        "LocalCuisinePreference",
                        "CulturalPreference"
                    }
                },
                {
                    "Seafood Delights", new List<string>
                    {
                        "SeafoodPreference"
                    }
                },
                {
                    "Street Food Adventures", new List<string>
                    {
                        "StreetFoodPreference",
                        "FoodTruckPreference"
                    }
                },
                {
                    "Gourmet Experiences", new List<string>
                    {
                        "FinePreference"
                    }
                },
                {
                    "Sweet Treats", new List<string>
                    {
                        "SweetPreference"
                    }
                },
                {
                    "Spicy Favorites", new List<string>
                    {
                        "SpicyPreference"
                    }
                },
                {
                    "Organic & Healthy", new List<string>
                    {
                        "OrganicPreference"
                    }
                },
                {
                    "Quick Service", new List<string>
                    {
                        "FastFoodPreference",
                        "CafeteriaPreference",
                        "DeliveryPreference"
                    }
                },
                {
                    "Buffet Extravaganza", new List<string>
                    {
                        "BuffetPreference"
                    }
                },
                {
                    "Dietary Preferences", new List<string>
                    {
                        "GlutenFreePreference",
                        "DairyFreePreference",
                        "NutFreePreference"
                    }
                },
                {
                    "Religious Dietary", new List<string>
                    {
                        "HalalPreference",
                        "KosherPreference"
                    }
                },
                {
                    "Taste Adventures", new List<string>
                    {
                        "SaltyPreference",
                        "SourPreference",
                        "BitterPreference",
                        "UmamiPreference"
                    }
                },
                {
                    "Special Considerations", new List<string>
                    {
                        "AllergiesPreference"
                    }
                },
                {
                    "Fusion & Modern", new List<string>
                    {
                        "FusionPreference"
                    }
                }
            };
        }

        public static class TravelGroups
        {
            public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> Groups =
                new Dictionary<string, IReadOnlyList<string>>
            {
                {
                    "Adventure & Sports", new List<string>
                    {
                        "AdventurePreference"
                    }
                },
                {
                    "Relaxation Retreats", new List<string>
                    {
                        "RelaxationPreference",
                        "SpaAndWellnessPreference"
                    }
                },
                {
                    "Cultural Highlights", new List<string>
                    {
                        "CulturalPreference"
                    }
                },
                {
                    "Nature Escapes", new List<string>
                    {
                        "NaturePreference",
                        "ForestPreference",
                        "MountainPreference"
                    }
                },
                {
                    "Urban Explorations", new List<string>
                    {
                        "UrbanPreference",
                        "CityCenterPreference"
                    }
                },
                {
                    "Beach & Water", new List<string>
                    {
                        "BeachPreference",
                        "WaterfrontPreference",
                        "IslandPreference",
                        "LakePreference",
                        "RiverPreference",
                        "WaterfallPreference"
                    }
                },
                {
                    "Travel Companions", new List<string>
                    {
                        "SoloTravelPreference",
                        "GroupTravelPreference",
                        "FamilyTravelPreference",
                        "CoupleTravelPreference"
                    }
                },
                {
                    "Budget Considerations", new List<string>
                    {
                        "LuxuryPreference",
                        "BudgetPreference"
                    }
                },
                {
                    "Rural & Remote", new List<string>
                    {
                        "RuralPreference",
                        "RemoteLocationPreference"
                    }
                },
                {
                    "Natural Wonders", new List<string>
                    {
                        "CavePreference",
                        "VolcanoPreference",
                        "GlacierPreference",
                        "CanyonPreference",
                        "ValleyPreference",
                        "DesertPreference"
                    }
                }
            };
        }
    }
}
