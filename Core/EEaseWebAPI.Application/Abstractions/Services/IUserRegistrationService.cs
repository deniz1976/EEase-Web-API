using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Application.Features.Commands.AppUser.CreateUser;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IUserRegistrationService
    {
        /// <summary>Returns the message the caller is shown once the account exists.</summary>
        Task<string> CreateAsync(CreateUser model, CancellationToken cancellationToken = default);

        Task<bool> SendVerificationEmailAgain(string email, CancellationToken cancellationToken = default);

        Task<bool> EmailConfirm(string code, string usernameOrEmail, CancellationToken cancellationToken = default);

        Task<bool> CheckEmailConfirmed(string emailOrUsername, CancellationToken cancellationToken = default);
    }
}
