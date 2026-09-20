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
        public string TargetUsername { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;
    }
}
