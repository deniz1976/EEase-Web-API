using EEaseWebAPI.Application.Enums;

namespace EEaseWebAPI.Application.Exceptions.Friendship
{
    public class UserBlockedException : FriendshipException
    {
        public UserBlockedException(string message) 
            : base(message, StatusEnum.UserBlockFailed)
        {
        }
    }
} 