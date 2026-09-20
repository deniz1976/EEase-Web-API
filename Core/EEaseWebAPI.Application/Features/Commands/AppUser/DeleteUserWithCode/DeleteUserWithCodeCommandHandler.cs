using EEaseWebAPI.Application.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using EEaseWebAPI.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.DeleteUserWithCode
{
    public class DeleteUserWithCodeCommandHandler : IRequestHandler<DeleteUserWithCodeCommandRequest, DeleteUserWithCodeCommandResponse>
    {
        private readonly IUserAccountService _accountService;
        private readonly IHeaderService _headerService;

        public DeleteUserWithCodeCommandHandler(IUserAccountService accountService, IHeaderService headerService)
        {
            _accountService = accountService;
            _headerService = headerService;
        }

        public async Task<DeleteUserWithCodeCommandResponse> Handle(DeleteUserWithCodeCommandRequest request, CancellationToken cancellationToken)
        {
// The service reports a refusal by throwing, with a reason the caller can read;
            // the bare Exception that stood in for "it came back empty" could only ever have
            // reached the caller as a 500 with nothing in it.
            var message = await _accountService.ConfirmDeletionAsync(request.username, request.code);

            return new DeleteUserWithCodeCommandResponse
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.UserDeletedSuccessfully),
                Body = new MapEntities.DeleteUserWithCode.DeleteUserWithCodeBody { message = message }
            };
        }
    }
}
