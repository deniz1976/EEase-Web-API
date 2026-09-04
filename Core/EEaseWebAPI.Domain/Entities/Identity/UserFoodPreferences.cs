using EEaseWebAPI.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Domain.Entities.Identity
{
    public class UserFoodPreferences : BaseEntity
    {
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser? User { get; set; }

        public int? VegetarianPreference { get; set; }
        public int? VeganPreference { get; set; }
        public int? GlutenFreePreference { get; set; }
        public int? HalalPreference { get; set; }
        public int? KosherPreference { get; set; }
        public int? SeafoodPreference { get; set; }
        public int? LocalCuisinePreference { get; set; }
        public int? FastFoodPreference { get; set; }
        public int? FinePreference { get; set; }
        public int? StreetFoodPreference { get; set; }
        public int? OrganicPreference { get; set; }
        public int? BuffetPreference { get; set; }
        public int? FoodTruckPreference { get; set; }
        public int? CafeteriaPreference { get; set; }
        public int? DeliveryPreference { get; set; }
        public int? AllergiesPreference { get; set; }
        public int? DairyFreePreference { get; set; }
        public int? NutFreePreference { get; set; }
        public int? SpicyPreference { get; set; }
        public int? SweetPreference { get; set; }
        public int? SaltyPreference { get; set; }
        public int? SourPreference { get; set; }
        public int? BitterPreference { get; set; }
        public int? UmamiPreference { get; set; }
        public int? FusionPreference { get; set; }
    }
}
