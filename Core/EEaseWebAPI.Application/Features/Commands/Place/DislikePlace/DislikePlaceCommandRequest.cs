using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.Place.DislikePlace
{
    public class DislikePlaceCommandRequest :IRequest<DislikePlaceCommandResponse>
    {
        public string Username { get; set; } = string.Empty;
        public string? RouteId { get; set; }
        public string GooglePlaceId { get; set; } = string.Empty;
        public string PlaceType { get; set; } = string.Empty;
        public string? UserFeedback { get; set; }
        public int? DislikeType { get; set; }
    }
}
