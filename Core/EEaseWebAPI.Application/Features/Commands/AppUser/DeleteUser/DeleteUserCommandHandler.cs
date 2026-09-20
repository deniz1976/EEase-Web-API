using EEaseWebAPI.Application.Abstractions.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Resources;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.DeleteUser
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommandRequest, DeleteUserCommandResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IUserAccountService _accountService;

        public DeleteUserCommandHandler(IHeaderService headerService, IUserAccountService accountService)
        {
            _headerService = headerService;
            _accountService = accountService;
        }

        public async Task<DeleteUserCommandResponse> Handle(DeleteUserCommandRequest request, CancellationToken cancellationToken)
        {
            var outcome = await _accountService.RequestDeletionAsync(request.Username);

            // Asking to delete an account that is already on its way out cancels the
            // deletion. That is an outcome of its own, not the failure it used to be
            // reported as, which read as "deletion failed" over a message saying the
            // account had been brought back.
            return outcome == DeleteRequestOutcome.CodeSent
                ? CreateResponse((int)StatusEnum.UserDeleteCodeSentSuccessfully, AppMessages.DeleteCodeSent)
                : CreateResponse((int)StatusEnum.AccountReactivated, AppMessages.AccountReactivated);
        }

        private DeleteUserCommandResponse CreateResponse(int headerCode, string message)
        {
            return new DeleteUserCommandResponse
            {
                Header = _headerService.HeaderCreate(headerCode),
                Body = new MapEntities.DeleteUser.DeleteUserBody
                {
                    Message = message
                }
            };
        }
    }
}
