using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.CancelFriendRequest
{
    public class CancelFriendRequestCommandRequest : IRequest<CancelFriendRequestCommandResponse>
    {
        public string? targetUsername { get; set; }

        public string? username { get; set; }
    }
}
