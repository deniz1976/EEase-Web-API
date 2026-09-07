namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IPasswordService
    {
        Task<bool> SendResetCodeAsync(string usernameOrEmail);

        Task<bool> VerifyResetCodeAsync(string usernameOrEmail, string code);

        Task ResetPasswordAsync(string usernameOrEmail, string code, string newPassword);

        Task<string> ChangePasswordAsync(string username, string oldPassword, string newPassword);
    }
}
