using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetAllTopics
{
    public class GetAllTopicsQueryHandler : IRequestHandler<GetAllTopicsQueryRequest, GetAllTopicsQueryResponse>
    {

        private readonly IUserService _userService;
        private readonly IHeaderService _headerService;

        public GetAllTopicsQueryHandler(IUserService userService,IHeaderService headerService)
        {
            _userService = userService;
            _headerService = headerService;
        }

        public async Task<GetAllTopicsQueryResponse> Handle(GetAllTopicsQueryRequest request, CancellationToken cancellationToken)
        {
            var responseBody = _userService.GetAllTopics();

            var response = new GetAllTopicsQueryResponse()
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.GetAllTopicsSuccessfully),
                Body = responseBody
            };


            return response;
        }
    }
}
