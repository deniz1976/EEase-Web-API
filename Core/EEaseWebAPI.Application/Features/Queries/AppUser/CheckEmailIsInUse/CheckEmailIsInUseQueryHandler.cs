using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
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
            if(request == null ||request.email == null)
                throw new ArgumentNullException(nameof(request));

            var isEmailInUse = await _authService.IsEmailInUse(request.email);
            var message = isEmailInUse ? "Email is in use." : "Email is free.";

            return new CheckEmailIsInUseQueryResponse()
            {
                CheckEmailIsInUse = new MapEntities.CheckEmailIsInUse.CheckEmailIsInUse()
                {
                    Header = _headerService.HeaderCreate(isEmailInUse ? (int)StatusEnum.EmailAlreadyInUse : (int)StatusEnum.SuccessfullyCreated),
                    Body = new MapEntities.CheckEmailIsInUse.CheckEmailIsInUseBody()
                    {
                        message = message,
                        result = isEmailInUse
                    }
                }
            };


        }
    }
}
