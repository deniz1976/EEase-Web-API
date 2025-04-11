using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.Friendship;
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
        private readonly IUserService _userService;
        private readonly IHeaderService _headerService;

        public UnblockUserCommandHandler(IUserService userService, IHeaderService headerService)
        {
            _userService = userService;
            _headerService = headerService;
        }

        public async Task<UnblockUserCommandResponse> Handle(UnblockUserCommandRequest request, CancellationToken cancellationToken)
        {
            if(request == null || request.username == null || request.targetUsername == null) 
            {
                throw new ArgumentNullException(nameof(request));
            }

            var result = await _userService.UnblockUserAsync(request.targetUsername, request.username);
            if (result) 
            {
                return new UnblockUserCommandResponse() 
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.UserUnblockedSuccessfully),
                    Body = new UnblockFriendCommandResponseBody() 
                    {
                        Message= "User unblocked successfully."
                    }
                    
                };
            }

            throw new UserUnblockedException("User Unblocked Failed.");
        }
    }
}
