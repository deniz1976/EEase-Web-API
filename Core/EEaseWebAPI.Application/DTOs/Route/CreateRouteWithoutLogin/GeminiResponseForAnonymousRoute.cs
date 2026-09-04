using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.DTOs.Route.CreateRouteWithoutLogin
{
    public class GeminiResponseForAnonymousRoute
    {
       public List<AnonymousDay>? AnonymousDayList { get; set; }
    }

    public class AnonymousDay
    {
        public string? DayDescription { get; set; }
        public string? AccomodationPlaceName { get; set; }
        public string? BreakfastPlaceName { get; set; }
        public string? LunchPlaceName { get; set; }
        public string? DinnerPlaceName { get; set; }
        public string? FirstPlaceName { get; set; }
        public string? SecondPlaceName { get; set; }
        public string? ThirdPlaceName { get; set; }
        public string? AfterDinnerPlaceName { get; set; }
        public string? ApproxPrice { get; set; }
    }
}
