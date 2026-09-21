using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Entities.Route;

namespace EEaseWebAPI.Persistence.Services.Gemini
{
    public static class GeminiPrompts
    {
        public const string EntityDescription = @"
You need to analyze the user's message and extract preferences according to these entity structures.
All preferences are scored on a scale of 0-100

IMPORTANT DIETARY RESTRICTIONS:
When analyzing food preferences, pay special attention to dietary restrictions. If a user mentions being:
- Vegan: Automatically set VeganPreference to 100 and set SeafoodPreference, DairyFreePreference to 0
- Vegetarian: Automatically set VegetarianPreference to 100 and set SeafoodPreference to 0
- Halal: Automatically set HalalPreference to 100
- Kosher: Automatically set KosherPreference to 100
- Gluten-Free: Automatically set GlutenFreePreference to 100
- Dairy-Free: Automatically set DairyFreePreference to 100
- Nut Allergy: Automatically set NutFreePreference to 100 and set AllergiesPreference to 100

1. UserAccommodationPreferences (all fields are int, 0-100 scale):
   - LuxuryHotelPreference: Preference for luxury hotels
   - BudgetHotelPreference: Preference for budget hotels
   - BoutiqueHotelPreference: Preference for boutique hotels
   - HostelPreference: Preference for hostels
   - ApartmentPreference: Preference for apartment stays
   - ResortPreference: Preference for resorts
   - VillaPreference: Preference for villas
   - GuestHousePreference: Preference for guesthouses
   - CampingPreference: Preference for camping
   - GlampingPreference: Preference for glamping
   - BedAndBreakfastPreference: Preference for bed and breakfast
   - AllInclusivePreference: Preference for all-inclusive hotels
   - SpaAndWellnessPreference: Preference for spa and wellness hotels
   - PetFriendlyPreference: Preference for pet-friendly accommodations
   - EcoFriendlyPreference: Preference for eco-friendly accommodations
   - RemoteLocationPreference: Preference for remote locations
   - CityCenterPreference: Preference for city center locations
   - FamilyFriendlyPreference: Preference for family-friendly accommodations
   - AdultsOnlyPreference: Preference for adults-only accommodations
   - HomestayPreference: Preference for homestays
   - WaterfrontPreference: Preference for waterfront locations
   - HistoricalBuildingPreference: Preference for historical buildings
   - AirbnbPreference: Preference for Airbnb accommodations
   - CoLivingSpacePreference: Preference for co-living spaces
   - ExtendedStayPreference: Preference for extended stay accommodations

2. UserFoodPreferences (all fields are int, 0-100 scale):
   - VegetarianPreference: Preference for vegetarian food (MUST be 100 if user is vegetarian)
   - VeganPreference: Preference for vegan food (MUST be 100 if user is vegan)
   - GlutenFreePreference: Preference for gluten-free food (MUST be 100 if user has celiac or gluten sensitivity)
   - HalalPreference: Preference for halal food (MUST be 100 if user requires halal)
   - KosherPreference: Preference for kosher food (MUST be 100 if user requires kosher)
   - SeafoodPreference: Preference for seafood (MUST be 0 for vegetarian/vegan users)
   - LocalCuisinePreference: Preference for local cuisine
   - FastFoodPreference: Preference for fast food
   - FinePreference: Preference for fine dining
   - StreetFoodPreference: Preference for street food
   - OrganicPreference: Preference for organic food
   - BuffetPreference: Preference for buffet dining
   - FoodTruckPreference: Preference for food trucks
   - CafeteriaPreference: Preference for cafeteria dining
   - DeliveryPreference: Preference for food delivery
   - AllergiesPreference: Consideration for food allergies (MUST be 100 if any allergy mentioned)
   - DairyFreePreference: Preference for dairy-free food (MUST be 100 if lactose intolerant)
   - NutFreePreference: Preference for nut-free food (MUST be 100 if nut allergy)
   - SpicyPreference: Preference for spicy food
   - SweetPreference: Preference for sweet food
   - SaltyPreference: Preference for salty food
   - SourPreference: Preference for sour food
   - BitterPreference: Preference for bitter food
   - UmamiPreference: Preference for umami food
   - FusionPreference: Preference for fusion cuisine

3. UserPersonalization (all fields are int, 0-100 scale):
   - AdventurePreference: Preference for adventure activities
   - RelaxationPreference: Preference for relaxation
   - CulturalPreference: Interest in cultural experiences
   - NaturePreference: Interest in nature activities
   - UrbanPreference: Preference for urban environments
   - RuralPreference: Preference for rural environments
   - LuxuryPreference: Preference for luxury experiences
   - BudgetPreference: Preference for budget-friendly options
   - SoloTravelPreference: Preference for solo travel
   - GroupTravelPreference: Preference for group travel
   - FamilyTravelPreference: Preference for family travel
   - CoupleTravelPreference: Preference for couple travel
   - BeachPreference: Interest in beach activities
   - MountainPreference: Interest in mountain activities
   - DesertPreference: Interest in desert environments
   - ForestPreference: Interest in forest environments
   - IslandPreference: Interest in island destinations
   - LakePreference: Interest in lake activities
   - RiverPreference: Interest in river activities
   - WaterfallPreference: Interest in waterfall destinations
   - CavePreference: Interest in cave exploration
   - VolcanoPreference: Interest in volcanic sites
   - GlacierPreference: Interest in glacier experiences
   - CanyonPreference: Interest in canyon exploration
   - ValleyPreference: Interest in valley destinations

CRITICAL RULES FOR DIETARY RESTRICTIONS:
1. Dietary restrictions MUST be treated as absolute rules, not preferences
2. If a user mentions being vegan, they CANNOT have seafood or dairy preferences
3. If a user mentions food allergies, those MUST be marked as 100 and considered in all recommendations
4. Religious dietary requirements (Halal/Kosher) MUST be strictly respected
5. When in doubt about a dietary restriction, ask for clarification rather than making assumptions
6. I Want you to think as a real human what user likes. Do not fill fields only 25 50 75 100, you can use any number between 0 - 100 , example: ValleyPreference = 53.
7. You have to choose at least 5 attributes per of these 3 personalization, food, accomodation";
        public static string UserPreferences(string message) =>
            $"{EntityDescription}\n\nUser Message: {message}\n\n" +
            "Analyze this message and return a JSON object with three sections: " +
            "accommodationPreferences, foodPreferences, and personalization. For each section, " +
            "include ONLY the relevant fields with scores from 0-100 based on the user's preferences. " +
            "Exclude any fields that cannot be confidently scored based on the message.";

        public static string PlacePreferences(string placeName, string placeType, string placeDescription, List<string> availablePreferences) =>
            $@"Analyze this place and determine which user preferences should be updated based on its characteristics.

Place Information:
- Name: {placeName}
- Type: {placeType}
- Description: {placeDescription}

Available Preferences to Update:
{string.Join("\n", availablePreferences.Select(p => $"- {p}"))}

Rules:
1. Return a list of preferences that should be updated based on the place's characteristics
2. Only select preferences that are strongly relevant to the place
3. Consider the place type, features, and overall experience
4. Do not include preferences that don't clearly match the place's characteristics
5. Return the response as a JSON array of preference names
6. If a hotel is recommended where there should be a restaurant, consider it as the hotel's restaurant.

Example Response Format:
[""PreferenceOne"", ""PreferenceTwo"", ""PreferenceThree""]

IMPORTANT:
- Return ONLY the JSON array, no additional text
- Only include preferences from the provided list
- Select preferences that have a clear connection to the place
- Do not include preferences just because they might be slightly relevant";
    }
}
