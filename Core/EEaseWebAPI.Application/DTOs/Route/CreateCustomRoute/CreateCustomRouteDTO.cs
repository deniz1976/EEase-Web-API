using EEaseWebAPI.Domain.Entities.Route;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.DTOs.Route.CreateCustomRoute
{
    public class CreateCustomRouteDTO
    {
        public List<string>? Usernames { get; set; } = new List<string>();

        public string? Destination { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public PRICE_LEVEL? PRICE_LEVEL { get; set; }
    }
}
