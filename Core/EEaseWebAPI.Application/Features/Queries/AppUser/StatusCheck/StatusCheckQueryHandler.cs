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

namespace EEaseWebAPI.Application.Features.Queries.AppUser.StatusCheck
{
    public class StatusCheckQueryHandler : IRequestHandler<StatusCheckQueryRequest, StatusCheckQueryResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IUserAccountService _accountService;

        public StatusCheckQueryHandler(IHeaderService headerService, IUserAccountService accountService)
        {
            _headerService = headerService;
            _accountService = accountService;
        }

        public async Task<StatusCheckQueryResponse> Handle(StatusCheckQueryRequest request, CancellationToken cancellationToken)
        {
            if(request == null ||request.username ==null)
                throw new ArgumentNullException(nameof(request));

            return new StatusCheckQueryResponse
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.GetUserStatusSuccessfully),
                Body = await _accountService.StatusCheck(request.username)
            };
        }
    }
}
