using EEaseWebAPI.Application.Features.Commands.AppUser.CreateUser;
using EEaseWebAPI.Application.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Validators.User
{
    public class CreateUserValidator : AbstractValidator<CreateUserCommandRequest>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(ValidationMessages.Name_Required)
                .MinimumLength(2).WithMessage(ValidationMessages.Name_TooShort);

            RuleFor(x => x.Surname)
                .NotEmpty().WithMessage(ValidationMessages.Surname_Required)
                .MinimumLength(2).WithMessage(ValidationMessages.Surname_TooShort);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(ValidationMessages.Email_Required)
                .EmailAddress().WithMessage(ValidationMessages.Email_Invalid);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(ValidationMessages.Password_Required)
                .MinimumLength(8).WithMessage(ValidationMessages.Password_TooShort)
                .Matches(@"[A-Z]").WithMessage(ValidationMessages.Password_NeedsUppercase)
                .Matches(@"[a-z]").WithMessage(ValidationMessages.Password_NeedsLowercase)
                .Matches(@"\d").WithMessage(ValidationMessages.Password_NeedsDigit)
                .Matches(@"[^\w\d\s:]").WithMessage(ValidationMessages.Password_NeedsSpecialCharacter)
                .Equal(x => x.PasswordConfirm).WithMessage(ValidationMessages.Password_DoesNotMatch);

            RuleFor(x => x.Gender)
                .Must(g => g == "Male" || g == "Female")
                .WithMessage(ValidationMessages.Gender_Invalid);

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage(ValidationMessages.Username_Required)
                .MinimumLength(3).WithMessage(ValidationMessages.Username_TooShort)
                .Matches(@"^[a-zA-Z0-9]+$").WithMessage(ValidationMessages.Username_InvalidCharacters);
        }
    }
}
