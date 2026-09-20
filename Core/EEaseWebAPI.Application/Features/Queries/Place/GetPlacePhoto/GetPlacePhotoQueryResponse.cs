using EEaseWebAPI.Application.MapEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.Place.GetPlacePhoto
{
    public class GetPlacePhotoQueryResponse : ApiResponse<GetPlacePhotoQueryResponseBody>
    {
    }

    public class GetPlacePhotoQueryResponseBody
    {
        public string? PhotoUri { get; set; }
    }
}
