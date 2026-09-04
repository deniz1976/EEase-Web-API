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
        private readonly IUserService _userService;

        public GetUserInfoQueryHandler(IHeaderService headerService, IUserService userService)
        {
            _headerService = headerService;
            _userService = userService;
        }

        public async Task<GetUserInfoQueryResponse> Handle(GetUserInfoQueryRequest request, CancellationToken cancellationToken)
        {
            if (request == null || request.username == null)
                throw new ArgumentNullException(nameof(request));

            DTOs.User.GetUserInfo response = await _userService.GetUserInfoQuery(request.username);

            return new GetUserInfoQueryResponse()
            {
                userInfo = new()
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.GetUserInfoSuccessfully),
                    GetUserInfoBody = new() 
                    {
                        name = response.name,
                        surname = response.surname,
                        gender = response.gender,
                        username = response.username,
                        email = response.email,
                        BornDate=response.borndate,
                        bio = response.bio,
                        photoPath = response.photoPath,
                        currency = response.currency,
                        country = response.country,
                        id = response.id,

                    }
                }
            };

        }
    }
}
