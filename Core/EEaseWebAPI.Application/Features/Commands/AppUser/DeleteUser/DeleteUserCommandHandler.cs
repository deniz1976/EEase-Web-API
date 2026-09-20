using EEaseWebAPI.Application.Abstractions.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EEaseWebAPI.Application.Enums;
using Microsoft.Extensions.Localization;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.DeleteUser
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommandRequest, DeleteUserCommandResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IUserAccountService _accountService;

        private readonly IStringLocalizer<AppMessages> _messages;


        public DeleteUserCommandHandler(IHeaderService headerService, IUserAccountService accountService,

            IStringLocalizer<AppMessages> messages)

        {
            _headerService = headerService;
            _accountService = accountService;

            _messages = messages;
        }

        public async Task<DeleteUserCommandResponse> Handle(DeleteUserCommandRequest request, CancellationToken cancellationToken)
        {
            if (request?.username == null)
                throw new ArgumentNullException(nameof(request));

            var outcome = await _accountService.RequestDeletionAsync(request.username);

            return outcome == DeleteRequestOutcome.CodeSent
                ? CreateResponse((int)StatusEnum.UserDeleteCodeSentSuccessfully, _messages["DeleteCodeSent"])
                : CreateResponse((int)StatusEnum.UserDeletionFailed, _messages["AccountReactivated"]);
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
