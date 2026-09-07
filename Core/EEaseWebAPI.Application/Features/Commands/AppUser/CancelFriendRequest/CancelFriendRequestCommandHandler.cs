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
        private readonly IFriendshipService _friendshipService;
        private readonly string message = "Friend request cancelled succesfully.";

        public CancelFriendRequestCommandHandler(IHeaderService headerService, IFriendshipService friendshipService)
        {
            _headerService = headerService;
            _friendshipService = friendshipService;
        }

        public async Task<CancelFriendRequestCommandResponse> Handle(CancelFriendRequestCommandRequest request, CancellationToken cancellationToken)
        {
            if(request == null || request.targetUsername == null || request.username == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            await _friendshipService.CancelRequestAsync(request.username, request.targetUsername);

            return new CancelFriendRequestCommandResponse()
            {
                Body = new()
                {
                    success = true,
                    message = message
                },
                Header = _headerService.HeaderCreate(((int)StatusEnum.FriendRequestCancelledSuccessfully))
            };

        }
    }
}
