using EEaseWebAPI.Application.MapEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.CancelFriendRequest
{
    public class CancelFriendRequestCommandResponse
    {
        public Header? Header { get; set; }

        public CancelFriendRequestCommandResponseBody? Body { get; set; }
    }

    public class CancelFriendRequestCommandResponseBody 
    {
        public string? message { get; set; }
        public bool? success { get; set; }
    }

}
