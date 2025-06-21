using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.CancelFriendRequest
{
    public class CancelFriendRequestCommandHandler : IRequestHandler<CancelFriendRequestCommandRequest, CancelFriendRequestCommandResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IUserService _userService;
        private readonly string message = "Friend request cancelled succesfully.";
        

        public CancelFriendRequestCommandHandler(IHeaderService headerService, IUserService userService)
        {
            _headerService = headerService;
            _userService = userService;
        }

        public async Task<CancelFriendRequestCommandResponse> Handle(CancelFriendRequestCommandRequest request, CancellationToken cancellationToken)
        {
            if(request == null || request.targetUsername == null || request.username == null) 
            {
                throw new ArgumentNullException(nameof(request));
            }

            return new CancelFriendRequestCommandResponse()
            {
                Body = new()
                {
                    success = await _userService.CancelFriendRequest(request.username, request.targetUsername),
                    message = message
                },
                Header = _headerService.HeaderCreate(((int)StatusEnum.FriendRequestCancelledSuccessfully))
            };
            
        }
    }
}
