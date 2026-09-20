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
        public string? Username { get; set; }
        public string? UserId { get; set; }
    }
}
