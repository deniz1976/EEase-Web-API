using System.Collections.Generic;

namespace EEaseWebAPI.Application.MapEntities.PreferenceGroups
{
    public class TravelPreferenceGroups
    {
        public class AccommodationGroups
        {
            public static readonly Dictionary<string, List<string>> Groups = new()
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

        public class FoodGroups
        {
            public static readonly Dictionary<string, List<string>> Groups = new()
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

        public class TravelGroups
        {
            public static readonly Dictionary<string, List<string>> Groups = new()
            {
                {
                    "Adventure & Sports", new List<string>
                    {
                        "AdventurePreference",
                        "SportsPreference"
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