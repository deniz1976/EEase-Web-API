using EEaseWebAPI.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Domain.Entities.Identity
{
    public class UserAccommodationPreferences : BaseEntity
    {
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser? User { get; set; }
        public int? LuxuryHotelPreference { get; set; }
        public int? BudgetHotelPreference { get; set; }
        public int? BoutiqueHotelPreference { get; set; }
        public int? HostelPreference { get; set; }
        public int? ApartmentPreference { get; set; }
        public int? ResortPreference { get; set; }
        public int? VillaPreference { get; set; }
        public int? GuestHousePreference { get; set; }
        public int? CampingPreference { get; set; }
        public int? GlampingPreference { get; set; }
        public int? BedAndBreakfastPreference { get; set; }
        public int? AllInclusivePreference { get; set; }
        public int? SpaAndWellnessPreference { get; set; }
        public int? PetFriendlyPreference { get; set; }
        public int? EcoFriendlyPreference { get; set; }
        public int? RemoteLocationPreference { get; set; }
        public int? CityCenterPreference { get; set; }
        public int? FamilyFriendlyPreference { get; set; }
        public int? AdultsOnlyPreference { get; set; }
        public int? HomestayPreference { get; set; }
        public int? WaterfrontPreference { get; set; }
        public int? HistoricalBuildingPreference { get; set; }
        public int? AirbnbPreference { get; set; }
        public int? CoLivingSpacePreference { get; set; }
        public int? ExtendedStayPreference { get; set; }
    }
}
