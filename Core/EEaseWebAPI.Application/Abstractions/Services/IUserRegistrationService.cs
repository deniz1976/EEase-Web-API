using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Application.Features.Commands.AppUser.CreateUser;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IUserRegistrationService
    {
        Task<CreateUserResponse> CreateAsync(CreateUser model);

        Task<bool> SendVerificationEmailAgain(string email);

        Task<bool> EmailConfirm(string code, string usernameOrEmail);

        Task<bool> CheckEmailConfirmed(string emailOrUsername);
    }
}
