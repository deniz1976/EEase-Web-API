using EEaseWebAPI.Application.Features.Commands.AppUser;
using EEaseWebAPI.Application.Features.Commands.AppUser.ChangePassword;
using EEaseWebAPI.Application.Features.Commands.AppUser.ConfirmEmailUser;
using EEaseWebAPI.Application.Features.Commands.AppUser.DeleteUserWithCode;
using EEaseWebAPI.Application.Features.Commands.AppUser.RefreshTokenLoginUser;
using EEaseWebAPI.Application.Features.Commands.AppUser.ResetPassword;
using EEaseWebAPI.Application.Features.Commands.AppUser.ResetPasswordUser;
using EEaseWebAPI.Application.Features.Commands.AppUser.SendVerificationCodeAgain;
using EEaseWebAPI.Application.Features.Commands.AppUser.SetUserPhoto;
using EEaseWebAPI.Application.Resources;
using FluentValidation;

namespace EEaseWebAPI.Application.Validators.User
{
    /// <summary>
    /// What a caller has to send to reach these handlers. The handlers used to check it
    /// themselves and answer "Value cannot be null. (Parameter 'request')", which named
    /// nothing the caller could fix and was never translated.
    /// </summary>
    public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommandRequest>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(request => request.OldPassword)
                .NotEmpty().WithMessage(ValidationMessages.OldPassword_Required);

            RuleFor(request => request.NewPassword)
                .NotEmpty().WithMessage(ValidationMessages.NewPassword_Required);
        }
    }

    public class ConfirmEmailUserCommandValidator : AbstractValidator<ConfirmEmailUserCommandRequest>
    {
        public ConfirmEmailUserCommandValidator()
        {
            RuleFor(request => request.Code)
                .NotEmpty().WithMessage(ValidationMessages.Code_Required);

            RuleFor(request => request.EmailOrUsername)
                .NotEmpty().WithMessage(ValidationMessages.EmailOrUsername_Required);
        }
    }

    public class DeleteUserWithCodeCommandValidator : AbstractValidator<DeleteUserWithCodeCommandRequest>
    {
        public DeleteUserWithCodeCommandValidator()
        {
            RuleFor(request => request.Code)
                .NotEmpty().WithMessage(ValidationMessages.Code_Required);
        }
    }

    public class RefreshTokenLoginUserCommandValidator : AbstractValidator<RefreshTokenLoginUserCommandRequest>
    {
        public RefreshTokenLoginUserCommandValidator()
        {
            RuleFor(request => request.RefreshToken)
                .NotEmpty().WithMessage(ValidationMessages.RefreshToken_Required);
        }
    }

    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommandRequest>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(request => request.UsernameOrEmail)
                .NotEmpty().WithMessage(ValidationMessages.EmailOrUsername_Required);

            RuleFor(request => request.Code)
                .NotEmpty().WithMessage(ValidationMessages.Code_Required);

            RuleFor(request => request.NewPassword)
                .NotEmpty().WithMessage(ValidationMessages.NewPassword_Required);
        }
    }

    public class ResetPasswordUserCommandValidator : AbstractValidator<ResetPasswordUserCommandRequest>
    {
        public ResetPasswordUserCommandValidator()
        {
            RuleFor(request => request.EmailOrUsername)
                .NotEmpty().WithMessage(ValidationMessages.EmailOrUsername_Required);
        }
    }

    public class SendVerificationCodeCommandValidator : AbstractValidator<SendVerificationCodeCommandRequest>
    {
        public SendVerificationCodeCommandValidator()
        {
            RuleFor(request => request.Email)
                .NotEmpty().WithMessage(ValidationMessages.Email_Required)
                .EmailAddress().WithMessage(ValidationMessages.Email_Invalid);
        }
    }

    public class SetUserPhotoCommandValidator : AbstractValidator<SetUserPhotoCommandRequest>
    {
        public SetUserPhotoCommandValidator()
        {
            RuleFor(request => request.PhotoUrl)
                .NotEmpty().WithMessage(ValidationMessages.PhotoPath_Required);
        }
    }
}
