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
                    request.PhotoName, request.MaxWidthPx, request.MaxHeightPx, cancellationToken),
                Header = _headerService.HeaderCreate((int)StatusEnum.RouteComponentPhotoRetrievedSuccessfully)
            };
        }

        private static void Validate(GetRouteComponentPhotoCommandRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PhotoName))
            {
                throw new ArgumentException("A photo name is required.", nameof(request));
            }

            if (request.MaxWidthPx <= 0 || request.MaxHeightPx <= 0 ||
                request.MaxWidthPx > MaximumPixels || request.MaxHeightPx > MaximumPixels)
            {
                throw new RouteComponentRequestOutOfRangeException();
            }
        }
    }
}
