using EEaseWebAPI.Application.MapEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.UnblockUser
{
    public class UnblockUserCommandResponse
    {
        public UnblockFriendCommandResponseBody? Body { get; set; }

        public Header? Header { get; set; }
    }

    public class UnblockFriendCommandResponseBody
    {
        public string? Message { get; set; } = "User unblocked successfully.";
    }
}
