using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.Friendship;
using EEaseWebAPI.Application.Resources;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.UnblockUser
{
    public class UnblockUserCommandHandler : IRequestHandler<UnblockUserCommandRequest, UnblockUserCommandResponse>
    {
        private readonly IFriendshipService _friendshipService;
        private readonly IHeaderService _headerService;

        public UnblockUserCommandHandler(IFriendshipService friendshipService, IHeaderService headerService)
        {
            _friendshipService = friendshipService;
            _headerService = headerService;
        }

        public async Task<UnblockUserCommandResponse> Handle(UnblockUserCommandRequest request, CancellationToken cancellationToken)
        {
            await _friendshipService.UnblockAsync(request.Username, request.TargetUsername);

            return new UnblockUserCommandResponse()
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.UserUnblockedSuccessfully),
                Body = new UnblockFriendCommandResponseBody()
                {
                    Message = AppMessages.UserUnblocked
                }
            };
        }
    }
}
