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
        public int MaxHeightPx { get; set; } = 1080;
        public int MaxWidthPx { get; set; } = 1920;
        public string PhotoName { get; set; } = string.Empty;
    }
}
