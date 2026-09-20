using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetUserInfoByName
{
    public class GetUserInfoByNameQueryRequest : IRequest<GetUserInfoByNameQueryResponse>
    {
        public string? Username { get; set; }
        public string? TargetUsername { get; set; }
    }
}
