using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.CheckFriendRequest
{
    public class CheckFriendRequestQueryRequest : IRequest<CheckFriendRequestQueryResponse>
    {
        public string Username { get; set; }
        public string TargetUsername { get; set; }
    }
}
