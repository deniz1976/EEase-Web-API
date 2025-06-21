using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetUserPhotoByName
{
    public class GetUserPhotoByNameQueryHandler : IRequestHandler<GetUserPhotoByNameQueryRequest, GetUserPhotoByNameQueryResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IUserService _userService;

        public GetUserPhotoByNameQueryHandler(IHeaderService headerService, IUserService userService)
        {
            _headerService = headerService;
            _userService = userService;
        }

        public async Task<GetUserPhotoByNameQueryResponse> Handle(GetUserPhotoByNameQueryRequest request, CancellationToken cancellationToken)
        {
            if(request == null || request.username == null || request.targetUsername == null)
                throw new ArgumentNullException(nameof(request));

            // If users are the same, return the photo directly
            if (request.username == request.targetUsername)
            {
                return new GetUserPhotoByNameQueryResponse() 
                {
                    response = new ()
                    {
                        Header = _headerService.HeaderCreate((int)StatusEnum.UserPhotoReceivedSuccessfully),
                        Body = new() { path = await _userService.GetUserPhotoAsync(request.targetUsername) }
                    }
                };
            }

            // Check if users are friends
            bool areFriends = await _userService.IsFriendAsync(request.username, request.targetUsername);
            
            if (!areFriends)
            {
                // If not friends, return a failed response
                return new GetUserPhotoByNameQueryResponse() 
                {
                    response = new ()
                    {
                        Header = _headerService.HeaderCreate((int)StatusEnum.NotFriends),
                        Body = new() { path = null, errorMessage = "You need to be friends with this user to view their photo." }
                    }
                };
            }

            // If friends, return the photo
            return new GetUserPhotoByNameQueryResponse() 
            {
                response = new ()
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.UserPhotoReceivedSuccessfully),
                    Body = new() { path = await _userService.GetUserPhotoAsync(request.targetUsername) }
                }
            };
        }
    }
}
