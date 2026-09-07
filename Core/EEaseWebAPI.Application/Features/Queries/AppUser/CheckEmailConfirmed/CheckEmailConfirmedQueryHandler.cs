using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.MapEntities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EEaseWebAPI.Application.Enums;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.CheckEmailConfirmed
{
    public class CheckEmailConfirmedQueryHandler : IRequestHandler<CheckEmailConfirmedQueryRequest, CheckEmailConfirmedQueryResponse>
    {
        private readonly IUserRegistrationService _registrationService;
        private readonly IHeaderService _headerService;
        public CheckEmailConfirmedQueryHandler(IUserRegistrationService registrationService,IHeaderService headerService)
        {
            _registrationService = registrationService;
            _headerService = headerService;
        }
        public async Task<CheckEmailConfirmedQueryResponse> Handle(CheckEmailConfirmedQueryRequest request, CancellationToken cancellationToken)
        {
            if(request == null || request.emailOrUsername == null)
                throw new ArgumentNullException(nameof(request));
            var result = await _registrationService.CheckEmailConfirmed(request.emailOrUsername);
            Header header = _headerService.HeaderCreate();
            return new CheckEmailConfirmedQueryResponse
            {
                response = new()
                {
                    Body = new CheckEmailBody { result = result},
                    Header = _headerService.HeaderCreate(result ? (int)StatusEnum.EmailConfirmed : (int)StatusEnum.EmailNotConfirmed)

                }
            };
        }
    }
}
