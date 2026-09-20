using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Application.Features.Commands.AppUser.CreateUser;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IUserRegistrationService
    {
        /// <summary>Returns the message the caller is shown once the account exists.</summary>
        Task<string> CreateAsync(CreateUser model);

        Task<bool> SendVerificationEmailAgain(string email);

        Task<bool> EmailConfirm(string code, string usernameOrEmail);

        Task<bool> CheckEmailConfirmed(string emailOrUsername);
    }
}
