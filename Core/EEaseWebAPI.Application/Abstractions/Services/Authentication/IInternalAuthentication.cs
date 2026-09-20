using EEaseWebAPI.Application.MapEntities.Login;
using EEaseWebAPI.Application.MapEntities.RefreshTokenLogin;

namespace EEaseWebAPI.Application.Abstractions.Services.Authentication
{
    public interface IInternalAuthentication
    {
        Task<LoginBody> LoginAsync(string usernameOrEmail, string password, int accessTokenLifetime, CancellationToken cancellationToken = default);

        Task<RefreshTokenLoginBody> RefreshTokenLoginAsync(string refreshToken, CancellationToken cancellationToken = default);

        Task<DTOs.Token> UpdateUserGetNewToken(string newUsername, CancellationToken cancellationToken = default);

        Task<bool> IsEmailInUse(string email, CancellationToken cancellationToken = default);
    }
}
