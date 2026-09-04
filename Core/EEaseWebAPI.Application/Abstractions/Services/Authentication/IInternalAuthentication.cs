using EEaseWebAPI.Application.MapEntities.Login;
using EEaseWebAPI.Application.MapEntities.RefreshTokenLogin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Abstractions.Services.Authentication
{
    public interface IInternalAuthentication
    {
        Task<LoginBody> LoginAsync(string usernameOrEmail, string password, int accessTokenLifetime);

        Task<RefreshTokenLoginBody> RefreshTokenLoginAsync(string refreshToken);

        Task<bool> ResetPassword(string usernameOrEmail);

        Task<bool> ResetPasswordCodeCheck(string code, string usernameOrEmail);

        Task ResetPasswordWithCode(string code, string newPassword);

        Task<string> ChangePassword(string username, string oldPassword, string newPassword);

        Task<DTOs.Token> UpdateUserGetNewToken(string newUsername);

        Task<bool> IsEmailInUse(string email);
    }
}
