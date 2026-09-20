using EEaseWebAPI.Application.Features.Commands.AppUser.BlockFriend;
using EEaseWebAPI.Application.Features.Commands.AppUser.CancelFriendRequest;
using EEaseWebAPI.Application.Features.Commands.AppUser.RemoveFriend;
using EEaseWebAPI.Application.Features.Commands.AppUser.RespondToFriendRequest;
using EEaseWebAPI.Application.Features.Commands.AppUser.SendFriendRequest;
using EEaseWebAPI.Application.Features.Commands.AppUser.UnblockUser;
using EEaseWebAPI.Application.Features.Queries.AppUser.CheckFriendRequest;
using EEaseWebAPI.Application.Resources;
using FluentValidation;

namespace EEaseWebAPI.Application.Validators.User
{
    /// <summary>
    /// Each of these names the other user. The caller's own name comes from the token, so
    /// there is nothing to validate about it.
    /// </summary>
    public class SendFriendRequestCommandValidator : AbstractValidator<SendFriendRequestCommandRequest>
    {
        public SendFriendRequestCommandValidator()
        {
            RuleFor(request => request.AddresseeUsername)
                .NotEmpty().WithMessage(ValidationMessages.TargetUsername_Required);
        }
    }

    public class RespondToFriendRequestCommandValidator : AbstractValidator<RespondToFriendRequestCommand>
    {
        public RespondToFriendRequestCommandValidator()
        {
            RuleFor(request => request.RequesterUsername)
                .NotEmpty().WithMessage(ValidationMessages.TargetUsername_Required);
        }
    }

    public class CancelFriendRequestCommandValidator : AbstractValidator<CancelFriendRequestCommandRequest>
    {
        public CancelFriendRequestCommandValidator()
        {
            RuleFor(request => request.TargetUsername)
                .NotEmpty().WithMessage(ValidationMessages.TargetUsername_Required);
        }
    }

    public class RemoveFriendCommandValidator : AbstractValidator<RemoveFriendCommand>
    {
        public RemoveFriendCommandValidator()
        {
            RuleFor(request => request.FriendUsername)
                .NotEmpty().WithMessage(ValidationMessages.TargetUsername_Required);
        }
    }

    public class BlockFriendCommandValidator : AbstractValidator<BlockFriendCommand>
    {
        public BlockFriendCommandValidator()
        {
            RuleFor(request => request.TargetUsername)
                .NotEmpty().WithMessage(ValidationMessages.TargetUsername_Required);
        }
    }

    public class UnblockUserCommandValidator : AbstractValidator<UnblockUserCommandRequest>
    {
        public UnblockUserCommandValidator()
        {
            RuleFor(request => request.TargetUsername)
                .NotEmpty().WithMessage(ValidationMessages.TargetUsername_Required);
        }
    }

    public class CheckFriendRequestQueryValidator : AbstractValidator<CheckFriendRequestQueryRequest>
    {
        public CheckFriendRequestQueryValidator()
        {
            RuleFor(request => request.TargetUsername)
                .NotEmpty().WithMessage(ValidationMessages.TargetUsername_Required);
        }
    }
}
