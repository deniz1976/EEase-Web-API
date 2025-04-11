using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.GetRouteComponent;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.Route.GetRouteComponentPhoto
{
    public class GetRouteComponentPhotoCommandHandler : IRequestHandler<GetRouteComponentPhotoCommandRequest, GetRouteComponentPhotoCommandResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IGooglePlacesService _googlePlacesService;

        public GetRouteComponentPhotoCommandHandler(IHeaderService headerService, IGooglePlacesService googlePlacesService)
        {
            _headerService = headerService;
            _googlePlacesService = googlePlacesService;
        }

        public async Task<GetRouteComponentPhotoCommandResponse> Handle(GetRouteComponentPhotoCommandRequest request, CancellationToken cancellationToken)
        {
            if (GetRouteComponentPhotoRequestControl(request))
            {
                return new GetRouteComponentPhotoCommandResponse()
                {
                    Body = await _googlePlacesService.GetPlacePhotosAsync(request.photoName, request.maxWidthPx, request.maxHeightPx),
                    Header = _headerService.HeaderCreate((int)StatusEnum.RouteComponentPhotoRetrievedSuccessfully)
                };
            }
            throw new Exception();
        }     
            
    

        public bool GetRouteComponentPhotoRequestControl(GetRouteComponentPhotoCommandRequest request)
        {
            if(request == null || request.photoName == null) throw new ArgumentNullException(nameof(request));

            if (request.maxWidthPx > 4800 || request.maxHeightPx > 4800 || request.maxWidthPx <= 0 || request.maxHeightPx <= 0) throw new RouteComponentRequestOutOfRangeException();

            return true;

        }
    }
}
