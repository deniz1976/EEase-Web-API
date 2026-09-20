using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.GetRouteComponent;
using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.Route.GetRouteComponentPhoto
{
    public class GetRouteComponentPhotoCommandHandler
        : IRequestHandler<GetRouteComponentPhotoCommandRequest, GetRouteComponentPhotoCommandResponse>
    {
        private const int MaximumPixels = 4800;

        private readonly IHeaderService _headerService;
        private readonly IGooglePlacesService _googlePlacesService;

        public GetRouteComponentPhotoCommandHandler(
            IHeaderService headerService, IGooglePlacesService googlePlacesService)
        {
            _headerService = headerService;
            _googlePlacesService = googlePlacesService;
        }

        public async Task<GetRouteComponentPhotoCommandResponse> Handle(
            GetRouteComponentPhotoCommandRequest request, CancellationToken cancellationToken)
        {
            Validate(request);

            return new GetRouteComponentPhotoCommandResponse
            {
                Body = await _googlePlacesService.GetPlacePhotosAsync(
                    request.photoName, request.maxWidthPx, request.maxHeightPx, cancellationToken),
                Header = _headerService.HeaderCreate((int)StatusEnum.RouteComponentPhotoRetrievedSuccessfully)
            };
        }

        private static void Validate(GetRouteComponentPhotoCommandRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.photoName))
            {
                throw new ArgumentException("A photo name is required.", nameof(request));
            }

            if (request.maxWidthPx <= 0 || request.maxHeightPx <= 0 ||
                request.maxWidthPx > MaximumPixels || request.maxHeightPx > MaximumPixels)
            {
                throw new RouteComponentRequestOutOfRangeException();
            }
        }
    }
}
