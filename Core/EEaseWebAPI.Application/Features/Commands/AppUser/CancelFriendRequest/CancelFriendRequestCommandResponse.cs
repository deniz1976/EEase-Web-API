using EEaseWebAPI.Application.MapEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.CancelFriendRequest
{
    public class CancelFriendRequestCommandResponse : ApiResponse<CancelFriendRequestCommandResponseBody>
    {
    }

    public class CancelFriendRequestCommandResponseBody
    {
        public string? Message { get; set; }
        public bool? Success { get; set; }
    }

}
