using System.ComponentModel;

namespace EEaseWebAPI.Domain.Enums
{
    public enum AccommodationPreferenceTypes
    {
        [Description("Luxury Lover")]
        LuxuryHotel,
        
        [Description("Budget Saver")]
        BudgetHotel,
        
        [Description("Boutique Explorer")]
        BoutiqueHotel,
        
        [Description("Backpacker")]
        Hostel,
        
        [Description("Apartment Seeker")]
        Apartment,
        
        [Description("Resort Enthusiast")]
        Resort,
        
        [Description("Villa Admirer")]
        Villa,
        
        [Description("Guesthouse Fan")]
        GuestHouse,
        
        [Description("Nature Camper")]
        Camping,
        
        [Description("Luxury Camper")]
        Glamping,
        
        [Description("B&B Lover")]
        BedAndBreakfast,
        
        [Description("All-Inclusive Seeker")]
        AllInclusive,
        
        [Description("Wellness Seeker")]
        SpaAndWellness,
        
        [Description("Pet Friendly Traveler")]
        PetFriendly,
        
        [Description("Eco Conscious")]
        EcoFriendly,
        
        [Description("Remote Seeker")]
        RemoteLocation,
        
        [Description("City Explorer")]
        CityCenter,
        
        [Description("Family Oriented")]
        FamilyFriendly,
        
        [Description("Adult Retreat Seeker")]
        AdultsOnly,
        
        [Description("Local Experience Seeker")]
        Homestay,
        
        [Description("Waterfront Lover")]
        Waterfront,
        
        [Description("History Enthusiast")]
        HistoricalBuilding,
        
        [Description("Home Sharer")]
        Airbnb,
        
        [Description("Community Living Fan")]
        CoLivingSpace,
        
        [Description("Long Stay Traveler")]
        ExtendedStay
    }
} 