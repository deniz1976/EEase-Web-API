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
            if(request == null || request.code == null || request.username == null)
                throw new ArgumentNullException(nameof(request));

            var message = await _accountService.ConfirmDeletionAsync(request.username,request.code);

            if(message != null)
            {
                return new DeleteUserWithCodeCommandResponse()
                {
                    DeleteUser = new MapEntities.DeleteUserWithCode.DeleteUserWithCode()
                    {
                        Header = _headerService.HeaderCreate((int)StatusEnum.UserDeletedSuccessfully),
                        Body = new MapEntities.DeleteUserWithCode.DeleteUserWithCodeBody()
                        {
                            message = message
                        }
                    }
                };
            }

            throw new Exception("An unexpected error occured.");
        }
    }
}
