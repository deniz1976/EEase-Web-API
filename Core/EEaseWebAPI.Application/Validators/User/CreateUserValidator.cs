using EEaseWebAPI.Application.Features.Commands.AppUser.CreateUser;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Validators.User
{
    public class CreateUserValidator : AbstractValidator<CreateUserCommandRequest>
    {
        public CreateUserValidator(IStringLocalizer<ValidationMessages> messages)
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(messages["Name_Required"])
                .MinimumLength(2).WithMessage(messages["Name_TooShort"]);

            RuleFor(x => x.Surname)
                .NotEmpty().WithMessage(messages["Surname_Required"])
                .MinimumLength(2).WithMessage(messages["Surname_TooShort"]);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(messages["Email_Required"])
                .EmailAddress().WithMessage(messages["Email_Invalid"]);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(messages["Password_Required"])
                .MinimumLength(8).WithMessage(messages["Password_TooShort"])
                .Matches(@"[A-Z]").WithMessage(messages["Password_NeedsUppercase"])
                .Matches(@"[a-z]").WithMessage(messages["Password_NeedsLowercase"])
                .Matches(@"\d").WithMessage(messages["Password_NeedsDigit"])
                .Matches(@"[^\w\d\s:]").WithMessage(messages["Password_NeedsSpecialCharacter"])
                .Equal(x => x.PasswordConfirm).WithMessage(messages["Password_DoesNotMatch"]);

            RuleFor(x => x.Gender)
                .Must(g => g == "Male" || g == "Female")
                .WithMessage(messages["Gender_Invalid"]);

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage(messages["Username_Required"])
                .MinimumLength(3).WithMessage(messages["Username_TooShort"])
                .Matches(@"^[a-zA-Z0-9]+$").WithMessage(messages["Username_InvalidCharacters"]);

        }
    }
}
