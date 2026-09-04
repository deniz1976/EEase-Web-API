using EEaseWebAPI.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Exceptions.Friendship
{
    public class UserUnblockedException : FriendshipException
    {

        public UserUnblockedException(string message)
            : base(message, StatusEnum.UserUnblockedFailed)
        {
        }
    }
}
