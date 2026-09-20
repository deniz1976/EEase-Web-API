using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Application.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetUserInfo
{
    public class GetUserInfoQueryHandler : IRequestHandler<GetUserInfoQueryRequest, GetUserInfoQueryResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IUserProfileService _profileService;

        public GetUserInfoQueryHandler(IHeaderService headerService, IUserProfileService profileService)
        {
            _headerService = headerService;
            _profileService = profileService;
        }

        public async Task<GetUserInfoQueryResponse> Handle(GetUserInfoQueryRequest request, CancellationToken cancellationToken)
        {
            DTOs.User.GetUserInfo response = await _profileService.GetUserInfoQuery(request.Username, cancellationToken);

            return new GetUserInfoQueryResponse()
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.GetUserInfoSuccessfully),
                Body = new()
                {
                    Name = response.Name,
                    Surname = response.Surname,
                    Gender = response.Gender,
                    Username = response.Username,
                    Email = response.Email,
                    BornDate=response.BornDate,
                    Bio = response.Bio,
                    PhotoPath = response.PhotoPath,
                    Currency = response.Currency,
                    Country = response.Country,
                    Id = response.Id,

                }
            };

        }
    }
}
