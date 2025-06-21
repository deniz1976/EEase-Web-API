using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetUserInfoById
{
    public class GetUserInfoByIdQueryRequest : IRequest<GetUserInfoByIdQueryResponse>
    {
        public string? username { get; set; }
        public string? userId { get; set; }
    }
} 