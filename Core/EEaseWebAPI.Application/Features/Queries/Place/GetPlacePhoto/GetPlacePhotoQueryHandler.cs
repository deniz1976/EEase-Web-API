using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.Place.GetPlacePhoto
{
    public class GetPlacePhotoQueryHandler
        : IRequestHandler<GetPlacePhotoQueryRequest, GetPlacePhotoQueryResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IGooglePlacesService _googlePlacesService;

        public GetPlacePhotoQueryHandler(
            IHeaderService headerService, IGooglePlacesService googlePlacesService)
        {
            _headerService = headerService;
            _googlePlacesService = googlePlacesService;
        }

        public async Task<GetPlacePhotoQueryResponse> Handle(
            GetPlacePhotoQueryRequest request, CancellationToken cancellationToken)
        {
            return new GetPlacePhotoQueryResponse
            {
                Body = await _googlePlacesService.GetPlacePhotosAsync(
                    request.PhotoName, request.MaxWidthPx, request.MaxHeightPx, cancellationToken),
                Header = _headerService.HeaderCreate((int)StatusEnum.RouteComponentPhotoRetrievedSuccessfully)
            };
        }

    }
}
