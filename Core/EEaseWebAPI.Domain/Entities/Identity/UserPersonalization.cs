using EEaseWebAPI.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Domain.Entities.Identity
{
    public class UserPersonalization : BaseEntity
    {
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser? User { get; set; }

        public int? AdventurePreference { get; set; }
        public int? RelaxationPreference { get; set; }
        public int? CulturalPreference { get; set; }
        public int? NaturePreference { get; set; }
        public int? UrbanPreference { get; set; }
        public int? RuralPreference { get; set; }
        public int? LuxuryPreference { get; set; }
        public int? BudgetPreference { get; set; }
        public int? SoloTravelPreference { get; set; }
        public int? GroupTravelPreference { get; set; }
        public int? FamilyTravelPreference { get; set; }
        public int? CoupleTravelPreference { get; set; }
        public int? BeachPreference { get; set; }
        public int? MountainPreference { get; set; }
        public int? DesertPreference { get; set; }
        public int? ForestPreference { get; set; }
        public int? IslandPreference { get; set; }
        public int? LakePreference { get; set; }
        public int? RiverPreference { get; set; }
        public int? WaterfallPreference { get; set; }
        public int? CavePreference { get; set; }
        public int? VolcanoPreference { get; set; }
        public int? GlacierPreference { get; set; }
        public int? CanyonPreference { get; set; }
        public int? ValleyPreference { get; set; }
    }
}
