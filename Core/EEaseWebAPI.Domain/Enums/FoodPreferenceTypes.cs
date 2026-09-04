using System.ComponentModel;

namespace EEaseWebAPI.Domain.Enums
{
    public enum FoodPreferenceTypes
    {
        [Description("Plant-Based Explorer")]
        Vegetarian,
        
        [Description("Vegan Enthusiast")]
        Vegan,
        
        [Description("Gluten-Free Diner")]
        GlutenFree,
        
        [Description("Halal Food Seeker")]
        Halal,
        
        [Description("Kosher Food Seeker")]
        Kosher,
        
        [Description("Seafood Lover")]
        Seafood,
        
        [Description("Local Food Explorer")]
        LocalCuisine,
        
        [Description("Quick Meal Seeker")]
        FastFood,
        
        [Description("Fine Dining Enthusiast")]
        Fine,
        
        [Description("Street Food Explorer")]
        StreetFood,
        
        [Description("Organic Food Lover")]
        Organic,
        
        [Description("Buffet Enthusiast")]
        Buffet,
        
        [Description("Food Truck Hunter")]
        FoodTruck,
        
        [Description("Cafeteria Diner")]
        Cafeteria,
        
        [Description("Delivery Fan")]
        Delivery,
        
        [Description("Allergy Conscious")]
        Allergies,
        
        [Description("Dairy-Free Diner")]
        DairyFree,
        
        [Description("Nut-Free Diner")]
        NutFree,
        
        [Description("Spicy Food Lover")]
        Spicy,
        
        [Description("Sweet Tooth")]
        Sweet,
        
        [Description("Salt Lover")]
        Salty,
        
        [Description("Sour Taste Fan")]
        Sour,
        
        [Description("Bitter Taste Appreciator")]
        Bitter,
        
        [Description("Umami Seeker")]
        Umami,
        
        [Description("Fusion Food Explorer")]
        Fusion
    }
} 