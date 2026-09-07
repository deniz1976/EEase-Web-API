using EEaseWebAPI.Application.MapEntities.Login;
using EEaseWebAPI.Application.MapEntities.RefreshTokenLogin;

namespace EEaseWebAPI.Application.Abstractions.Services.Authentication
{
    public interface IInternalAuthentication
    {
        Task<LoginBody> LoginAsync(string usernameOrEmail, string password, int accessTokenLifetime);

        Task<RefreshTokenLoginBody> RefreshTokenLoginAsync(string refreshToken);

        Task<DTOs.Token> UpdateUserGetNewToken(string newUsername);

        Task<bool> IsEmailInUse(string email);
    }
}
