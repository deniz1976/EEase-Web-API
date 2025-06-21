using EEaseWebAPI.Domain.Entities.Common;
using EEaseWebAPI.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Domain.Entities.Route
{
    public class TravelDay : BaseEntity
    {
        
        public string? DayDescription { get; set; }
        public AppUser? User { get; set; }
        public TravelAccomodation? Accomodation { get; set; }
        public Breakfast? Breakfast { get; set; }
        public Lunch? Lunch { get; set; }
        public Dinner? Dinner { get; set; }
        public Place? FirstPlace { get; set; } 
        public Place? SecondPlace { get; set; } 
        public Place? ThirdPlace { get; set; }
        public PlaceAfterDinner? PlaceAfterDinner { get; set; } 
        public string? approxPrice { get; set; }
    }
}
