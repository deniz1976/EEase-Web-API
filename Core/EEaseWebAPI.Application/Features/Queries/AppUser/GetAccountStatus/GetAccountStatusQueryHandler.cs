using EEaseWebAPI.Application.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EEaseWebAPI.Application.Enums;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetAccountStatus
{
    public class GetAccountStatusQueryHandler : IRequestHandler<GetAccountStatusQueryRequest, GetAccountStatusQueryResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IUserAccountService _accountService;

        public GetAccountStatusQueryHandler(IHeaderService headerService, IUserAccountService accountService)
        {
            _headerService = headerService;
            _accountService = accountService;
        }

        public async Task<GetAccountStatusQueryResponse> Handle(GetAccountStatusQueryRequest request, CancellationToken cancellationToken)
        {
            return new GetAccountStatusQueryResponse
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.GetUserStatusSuccessfully),
                Body = await _accountService.GetAccountStatusAsync(request.Username, cancellationToken)
            };
        }
    }
}
