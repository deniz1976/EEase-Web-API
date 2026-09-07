namespace EEaseWebAPI.Application.DTOs.User
{
    public sealed record AccountStatus(bool IsDeleted, string Message)
    {
        public static AccountStatus Active { get; } = new(false, "Account is active.");
    }
}
