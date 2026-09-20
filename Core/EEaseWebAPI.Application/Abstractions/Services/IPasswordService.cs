namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IPasswordService
    {
        Task<bool> SendResetCodeAsync(string usernameOrEmail, CancellationToken cancellationToken = default);

        Task<bool> VerifyResetCodeAsync(string usernameOrEmail, string code, CancellationToken cancellationToken = default);

        Task ResetPasswordAsync(string usernameOrEmail, string code, string newPassword, CancellationToken cancellationToken = default);

        Task<string> ChangePasswordAsync(string username, string oldPassword, string newPassword, CancellationToken cancellationToken = default);
    }
}
