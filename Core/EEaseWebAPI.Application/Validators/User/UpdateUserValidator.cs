using EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUser;
using EEaseWebAPI.Application.Resources;
using FluentValidation;

namespace EEaseWebAPI.Application.Validators.User
{
    /// <summary>
    /// Every field is optional here: an update carries only what changed, so each rule runs
    /// only when its field was sent. Without this the service was the first thing to
    /// complain, and it complained with an untranslated sentence and no field name.
    /// </summary>
    public class UpdateUserValidator : AbstractValidator<UpdateUserCommandRequest>
    {
        public UpdateUserValidator()
        {
            RuleFor(request => request.Username)
                .MinimumLength(UserProfileRules.UsernameMinLength)
                    .WithMessage(ValidationMessages.Username_TooShort)
                .Matches(@"^[a-zA-Z0-9]+$")
                    .WithMessage(ValidationMessages.Username_InvalidCharacters)
                .When(request => request.Username != null);

            RuleFor(request => request.Name)
                .MinimumLength(UserProfileRules.NameMinLength)
                    .WithMessage(ValidationMessages.Name_TooShort)
                .MaximumLength(UserProfileRules.NameMaxLength)
                    .WithMessage(string.Format(ValidationMessages.Name_TooLong, UserProfileRules.NameMaxLength))
                .When(request => request.Name != null);

            RuleFor(request => request.Surname)
                .MinimumLength(UserProfileRules.NameMinLength)
                    .WithMessage(ValidationMessages.Surname_TooShort)
                .MaximumLength(UserProfileRules.NameMaxLength)
                    .WithMessage(string.Format(ValidationMessages.Surname_TooLong, UserProfileRules.NameMaxLength))
                .When(request => request.Surname != null);

            RuleFor(request => request.bio)
                .MaximumLength(UserProfileRules.BioMaxLength)
                    .WithMessage(string.Format(ValidationMessages.Bio_TooLong, UserProfileRules.BioMaxLength))
                .When(request => request.bio != null);

            RuleFor(request => request.Gender)
                .Must(UserProfileRules.IsKnownGender)
                    .WithMessage(ValidationMessages.Gender_Invalid)
                .When(request => request.Gender != null);

            RuleFor(request => request.BornDate)
                .Must(bornDate => UserProfileRules.IsOldEnough(bornDate!.Value))
                    .WithMessage(string.Format(ValidationMessages.BornDate_TooYoung, UserProfileRules.MinimumAge))
                .When(request => request.BornDate != null);
        }
    }
}
