using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Resources;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.CheckEmailIsInUse
{
    public class CheckEmailIsInUseQueryHandler : IRequestHandler<CheckEmailIsInUseQueryRequest, CheckEmailIsInUseQueryResponse>
    {
        private readonly IAuthService _authService;
        private readonly IHeaderService _headerService;

        public CheckEmailIsInUseQueryHandler(IAuthService authService, IHeaderService headerService)
        {
            _authService = authService;
            _headerService = headerService;
        }

        public async Task<CheckEmailIsInUseQueryResponse> Handle(CheckEmailIsInUseQueryRequest request, CancellationToken cancellationToken)
        {
            var isEmailInUse = await _authService.IsEmailInUse(request.Email);
            var message = isEmailInUse ? AppMessages.EmailInUse : AppMessages.EmailAvailable;

            return new CheckEmailIsInUseQueryResponse()
            {
                Header = _headerService.HeaderCreate(isEmailInUse ? (int)StatusEnum.EmailAlreadyInUse : (int)StatusEnum.SuccessfullyCreated),
                Body = new MapEntities.CheckEmailIsInUse.CheckEmailIsInUseBody()
                {
                    Message = message,
                    Result = isEmailInUse
                }
            };
        }
    }
}
