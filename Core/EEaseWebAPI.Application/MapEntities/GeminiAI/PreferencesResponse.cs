using EEaseWebAPI.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.MapEntities.GeminiAI
{
    public class PreferencesResponse
    {
        public UserAccommodationPreferences? AccommodationPreferences { get; set; }
        public UserFoodPreferences? FoodPreferences { get; set; }
        public UserPersonalization? Personalization { get; set; }
    }
} 