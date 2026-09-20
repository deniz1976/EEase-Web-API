using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.Place.LikePlace
{
    public class LikePlaceCommandRequest : IRequest<LikePlaceCommandResponse>
    {
        public string GooglePlaceId { get; set; } = string.Empty;

        public string PlaceType { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;
    }
}
