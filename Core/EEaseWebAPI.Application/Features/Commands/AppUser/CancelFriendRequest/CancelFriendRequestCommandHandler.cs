using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Resources;
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
        

        public CancelFriendRequestCommandHandler(IHeaderService headerService, IFriendshipService friendshipService)
        {
            _headerService = headerService;
            _friendshipService = friendshipService;
        }

        public async Task<CancelFriendRequestCommandResponse> Handle(CancelFriendRequestCommandRequest request, CancellationToken cancellationToken)
        {
            await _friendshipService.CancelRequestAsync(request.Username, request.TargetUsername);

            return new CancelFriendRequestCommandResponse()
            {
                Body = new()
                {
                    Success = true,
                    Message = AppMessages.FriendRequestCancelled
                },
                Header = _headerService.HeaderCreate(((int)StatusEnum.FriendRequestCancelledSuccessfully))
            };
        }
    }
}
