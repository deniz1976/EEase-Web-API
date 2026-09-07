using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Abstractions.Services.Authentication;
using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.CreateUser;
using EEaseWebAPI.Application.Features.Commands.AppUser.CreateUser;
using EEaseWebAPI.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using System.Globalization;

namespace EEaseWebAPI.Persistence.Services.User
{
    public class UserRegistrationService : IUserRegistrationService
    {
        public const int MinimumAge = 13;

        private static readonly CultureInfo NameCulture = CultureInfo.GetCultureInfo("tr-TR");

        private readonly UserManager<AppUser> _userManager;
        private readonly IHeaderService _headerService;
        private readonly IMailService _mailService;
        private readonly IUserCacheService _userCacheService;
        private readonly IVerificationCodeGenerator _codeGenerator;

        public UserRegistrationService(
            UserManager<AppUser> userManager,
            IHeaderService headerService,
            IMailService mailService,
            IUserCacheService userCacheService,
            IVerificationCodeGenerator codeGenerator)
        {
            _userManager = userManager;
            _headerService = headerService;
            _mailService = mailService;
            _userCacheService = userCacheService;
            _codeGenerator = codeGenerator;
        }

        public async Task<CreateUserResponse> CreateAsync(CreateUser model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                throw new CreateUserFailedException("This email is already in use", (int)StatusEnum.EmailAlreadyInUse);
            }

            if (await _userManager.FindByNameAsync(model.Username) != null)
            {
                throw new CreateUserFailedException("This username is already in use", (int)StatusEnum.InvalidUsername);
            }

            if (model.BornDate.HasValue && AgeOn(model.BornDate.Value) < MinimumAge)
            {
                throw new CreateUserFailedException(
                    $"Users must be at least {MinimumAge} years old to register", (int)StatusEnum.InvalidAge);
            }

            var verificationCode = _codeGenerator.Generate();

            var newUser = new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                Name = Normalize(model.Name),
                Email = model.Email,
                Surname = Normalize(model.Surname),
                UserName = model.Username,
                Gender = model.Gender,
                VerificationCode = verificationCode,
                BornDate = model.BornDate
            };

            var result = await _userManager.CreateAsync(newUser, model.Password);

            if (!result.Succeeded)
            {
                throw new CreateUserFailedException(
                    $"Failed to create user: {Describe(result)}", (int)StatusEnum.UserUpdateFailed);
            }

            _mailService.SendVerificationEmail(model.Email, "Email Confirm", verificationCode);

            return new CreateUserResponse
            {
                response = new Application.MapEntities.CreateUser.CreateUser
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.SuccessfullyCreated),
                    Body = new Application.MapEntities.CreateUser.CreateUserBody
                    {
                        message = "Account created successfully. You must confirm email before use."
                    }
                }
            };
        }

        public async Task<bool> SendVerificationEmailAgain(string email)
        {
            var user = await FindAsync(email);

            if (user.EmailConfirmed)
            {
                throw new Application.Exceptions.EmailConfirmException(
                    "Email is already confirmed", (int)StatusEnum.EmailConfirmed);
            }

            user.VerificationCode = _codeGenerator.Generate();

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                throw new CreateUserFailedException(
                    $"Failed to store the verification code: {Describe(result)}", (int)StatusEnum.UserUpdateFailed);
            }

            _mailService.SendVerificationEmail(user.Email, "Email Confirm", user.VerificationCode);

            return true;
        }

        public async Task<bool> EmailConfirm(string code, string usernameOrEmail)
        {
            var user = await FindAsync(usernameOrEmail);

            if (user.EmailConfirmed)
            {
                throw new Application.Exceptions.EmailConfirmException(
                    "Email is already confirmed", (int)StatusEnum.EmailConfirmed);
            }

            if (user.VerificationCode == null || user.VerificationCode != code)
            {
                throw new Application.Exceptions.EmailConfirmException(
                    "Code is not correct", (int)StatusEnum.InvalidEmailConfirmationCode);
            }

            user.EmailConfirmed = true;
            user.VerificationCode = null;

            await _userManager.UpdateAsync(user);
            _userCacheService.AddOrUpdateUserInCache(user);

            return true;
        }

        public async Task<bool> CheckEmailConfirmed(string emailOrUsername) =>
            (await FindAsync(emailOrUsername)).EmailConfirmed;

        private async Task<AppUser> FindAsync(string emailOrUsername)
        {
            if (string.IsNullOrWhiteSpace(emailOrUsername))
            {
                throw new Application.Exceptions.Login.UserNotFoundException(
                    "User not found", (int)StatusEnum.UserNotFound);
            }

            return await _userManager.FindByEmailAsync(emailOrUsername)
                ?? await _userManager.FindByNameAsync(emailOrUsername)
                ?? throw new Application.Exceptions.Login.UserNotFoundException(
                    "User not found", (int)StatusEnum.UserNotFound);
        }

        public static int AgeOn(DateOnly bornDate)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - bornDate.Year;

            if (bornDate.AddYears(age) > today)
            {
                age--;
            }

            return age;
        }

        private static string? Normalize(string? input) =>
            string.IsNullOrWhiteSpace(input)
                ? input
                : char.ToUpper(input[0], NameCulture) + input[1..].ToLower(NameCulture);

        private static string Describe(IdentityResult result) =>
            string.Join("; ", result.Errors.Select(error => error.Description));
    }
}
