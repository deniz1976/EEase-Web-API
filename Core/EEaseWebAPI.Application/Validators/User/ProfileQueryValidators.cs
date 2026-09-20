using EEaseWebAPI.Application.Features.Queries.AppUser.CheckEmailConfirmed;
using EEaseWebAPI.Application.Features.Queries.AppUser.CheckEmailIsInUse;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetUserInfoById;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetUserInfoByName;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetUserPhotoByName;
using EEaseWebAPI.Application.Features.Queries.AppUser.ResetPasswordCodeCheck;
using EEaseWebAPI.Application.Resources;
using FluentValidation;

namespace EEaseWebAPI.Application.Validators.User
{
    public class CheckEmailConfirmedQueryValidator : AbstractValidator<CheckEmailConfirmedQueryRequest>
    {
        public CheckEmailConfirmedQueryValidator()
        {
            RuleFor(request => request.EmailOrUsername)
                .NotEmpty().WithMessage(ValidationMessages.EmailOrUsername_Required);
        }
    }

    public class CheckEmailIsInUseQueryValidator : AbstractValidator<CheckEmailIsInUseQueryRequest>
    {
        public CheckEmailIsInUseQueryValidator()
        {
            RuleFor(request => request.Email)
                .NotEmpty().WithMessage(ValidationMessages.Email_Required)
                .EmailAddress().WithMessage(ValidationMessages.Email_Invalid);
        }
    }

    public class ResetPasswordCodeCheckQueryValidator : AbstractValidator<ResetPasswordCodeCheckQueryRequest>
    {
        public ResetPasswordCodeCheckQueryValidator()
        {
            RuleFor(request => request.UsernameOrEmail)
                .NotEmpty().WithMessage(ValidationMessages.EmailOrUsername_Required);
        }
    }

    public class GetUserInfoByIdQueryValidator : AbstractValidator<GetUserInfoByIdQueryRequest>
    {
        public GetUserInfoByIdQueryValidator()
        {
            RuleFor(request => request.UserId)
                .NotEmpty().WithMessage(ValidationMessages.UserId_Required);
        }
    }

    public class GetUserInfoByNameQueryValidator : AbstractValidator<GetUserInfoByNameQueryRequest>
    {
        public GetUserInfoByNameQueryValidator()
        {
            RuleFor(request => request.TargetUsername)
                .NotEmpty().WithMessage(ValidationMessages.TargetUsername_Required);
        }
    }

    public class GetUserPhotoByNameQueryValidator : AbstractValidator<GetUserPhotoByNameQueryRequest>
    {
        public GetUserPhotoByNameQueryValidator()
        {
            RuleFor(request => request.TargetUsername)
                .NotEmpty().WithMessage(ValidationMessages.TargetUsername_Required);
        }
    }
}
