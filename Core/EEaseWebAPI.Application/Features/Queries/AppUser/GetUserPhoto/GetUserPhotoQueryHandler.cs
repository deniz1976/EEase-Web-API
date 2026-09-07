using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetUserPhoto
{
    public class GetUserPhotoQueryHandler : IRequestHandler<GetUserPhotoQueryRequest, GetUserPhotoQueryResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IUserProfileService _profileService;

        public GetUserPhotoQueryHandler(IHeaderService headerService, IUserProfileService profileService)
        {
            _headerService = headerService;
            _profileService = profileService;
        }

        public async Task<GetUserPhotoQueryResponse> Handle(GetUserPhotoQueryRequest request, CancellationToken cancellationToken)
        {
            if(request == null || request.username == null) { throw new ArgumentNullException(nameof(request)); }
            return new GetUserPhotoQueryResponse()
            {
                response = new()
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.UserPhotoReceivedSuccessfully),
                    Body = new() { path= await _profileService.GetUserPhotoAsync(request.username) }
                }
            };
        }
    }
}
