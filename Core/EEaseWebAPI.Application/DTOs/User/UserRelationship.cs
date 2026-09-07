using EEaseWebAPI.Application.Enums;

namespace EEaseWebAPI.Application.DTOs.User
{
    public sealed record UserRelationship(
        ProfileVisibilityStatus Visibility,
        FriendRequestStatus RequestStatus)
    {
        public static UserRelationship Self { get; } =
            new(ProfileVisibilityStatus.FullAccess, FriendRequestStatus.NoRequest);

        public bool HasFullAccess => Visibility == ProfileVisibilityStatus.FullAccess;
    }
}
