using EEaseWebAPI.Application.MapEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetAllTopics
{
    public class GetAllTopicsQueryResponse
    {
        public Header? Header { get; set; }
        public GetAllTopicsQueryResponseBody? Body { get; set; }
    }

    public class GetAllTopicsQueryResponseBody 
    {
        public List<string> AccommodationTopics { get; set; } = new List<string>();
        public List<string> FoodTopics { get; set; } = new List<string>();
        public List<string> TravelTopics { get; set; } = new List<string>();
    }
}
