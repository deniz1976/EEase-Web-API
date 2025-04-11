using EEaseWebAPI.Application.Abstractions.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EEaseWebAPI.Application.Enums;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.DeleteUser
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommandRequest, DeleteUserCommandResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IUserService _userService;
        

        public DeleteUserCommandHandler(IHeaderService headerService, IUserService userService)
        {
            _headerService = headerService;
            _userService = userService;
        }

        public async Task<DeleteUserCommandResponse> Handle(DeleteUserCommandRequest request, CancellationToken cancellationToken)
        {
            if (request?.username == null)
                throw new ArgumentNullException(nameof(request));

            var isDeleted = await _userService.DeleteUserSendMail(request.username);

            return isDeleted
                ? CreateResponse((int)StatusEnum.UserDeleteCodeSentSuccessfully, "Delete code sent to mail.")
                : CreateResponse((int)StatusEnum.UserDeletionFailed, "User status set to active.");
        }

        private DeleteUserCommandResponse CreateResponse(int headerCode, string message)
        {
            return new DeleteUserCommandResponse
            {
                DeleteUser = new MapEntities.DeleteUser.DeleteUser
                {
                    Header = _headerService.HeaderCreate(headerCode),
                    Body = new MapEntities.DeleteUser.DeleteUserBody
                    {
                        message = message
                    }
                }
            };
        }
    }
}
