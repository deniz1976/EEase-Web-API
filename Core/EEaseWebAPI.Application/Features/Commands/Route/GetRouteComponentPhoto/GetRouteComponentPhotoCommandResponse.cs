using EEaseWebAPI.Application.MapEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.Route.GetRouteComponentPhoto
{
    public class GetRouteComponentPhotoCommandResponse
    {
        public Header? Header { get; set; }
        public GetRouteComponentPhotoCommandResponseBody? Body { get; set; }
    }

    public class GetRouteComponentPhotoCommandResponseBody
    {
        public string? photoUri { get; set; }
    }
}
