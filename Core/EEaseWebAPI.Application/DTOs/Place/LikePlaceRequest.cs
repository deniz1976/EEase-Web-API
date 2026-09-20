using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.DTOs.Place
{
    public class LikePlaceRequest
    {
        public string GooglePlaceId { get; set; } = string.Empty;
        public string PlaceType { get; set; } = string.Empty;
    }
}
