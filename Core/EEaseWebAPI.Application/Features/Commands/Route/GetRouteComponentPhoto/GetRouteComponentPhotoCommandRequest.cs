using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.Route.GetRouteComponentPhoto
{
    public class GetRouteComponentPhotoCommandRequest : IRequest<GetRouteComponentPhotoCommandResponse>
    {
        public int maxHeightPx { get; set; } = 1080;
        public int maxWidthPx { get; set; } = 1920;
        public string? photoName { get; set; }
    }
}
