using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Features.Commands.AppUser.DeleteUser;
using EEaseWebAPI.Application.Features.Queries.AppUser.CheckEmailConfirmed;
using EEaseWebAPI.Application.Features.Queries.AppUser.CheckEmailIsInUse;
using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Application.Resources;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace EEaseWebAPI.UnitTests.User
{
    public class AccountHandlerTests
    {
        private readonly IHeaderService _headers = Substitute.For<IHeaderService>();
        private readonly IUserAccountService _account = Substitute.For<IUserAccountService>();
        private readonly IUserRegistrationService _registration = Substitute.For<IUserRegistrationService>();
        private readonly IAuthService _auth = Substitute.For<IAuthService>();

        public AccountHandlerTests()
        {
            _headers.HeaderCreate(Arg.Any<int>()).Returns(call => new Header
            {
                Success = true,
                EnumStatusCode = call.Arg<int>()
            });
        }

        [Fact]
        public async Task Asking_to_delete_an_account_sends_a_code()
        {
            _account.RequestDeletionAsync("alice").Returns(DeleteRequestOutcome.CodeSent);

            var response = await new DeleteUserCommandHandler(_headers, _account)
                .Handle(new DeleteUserCommandRequest { Username = "alice" }, CancellationToken.None);

            response.Header!.EnumStatusCode.Should().Be((int)StatusEnum.UserDeleteCodeSentSuccessfully);
            response.Body!.Message.Should().Be(AppMessages.DeleteCodeSent);
        }

        [Fact]
        public async Task Asking_again_while_a_deletion_is_pending_brings_the_account_back()
        {
            _account.RequestDeletionAsync("alice").Returns(DeleteRequestOutcome.Reactivated);

            var response = await new DeleteUserCommandHandler(_headers, _account)
                .Handle(new DeleteUserCommandRequest { Username = "alice" }, CancellationToken.None);

            // It used to be reported as UserDeletionFailed, over a message saying the
            // account had been reactivated.
            response.Header!.EnumStatusCode.Should().Be((int)StatusEnum.AccountReactivated);
            response.Body!.Message.Should().Be(AppMessages.AccountReactivated);
        }

        [Theory]
        [InlineData(true, StatusEnum.EmailConfirmed)]
        [InlineData(false, StatusEnum.EmailNotConfirmed)]
        public async Task A_confirmed_address_and_an_unconfirmed_one_are_told_apart(
            bool confirmed, StatusEnum expected)
        {
            _registration.CheckEmailConfirmed("alice@example.com").Returns(confirmed);

            var response = await new CheckEmailConfirmedQueryHandler(_registration, _headers)
                .Handle(
                    new CheckEmailConfirmedQueryRequest { EmailOrUsername = "alice@example.com" },
                    CancellationToken.None);

            response.Header!.EnumStatusCode.Should().Be((int)expected);
            response.Body!.Result.Should().Be(confirmed);
        }

        [Theory]
        [InlineData(true, StatusEnum.EmailAlreadyInUse)]
        [InlineData(false, StatusEnum.SuccessfullyCreated)]
        public async Task An_address_that_is_taken_is_reported_as_taken(bool inUse, StatusEnum expected)
        {
            _auth.IsEmailInUse("alice@example.com").Returns(inUse);

            var response = await new CheckEmailIsInUseQueryHandler(_auth, _headers)
                .Handle(
                    new CheckEmailIsInUseQueryRequest { Email = "alice@example.com" },
                    CancellationToken.None);

            response.Header!.EnumStatusCode.Should().Be((int)expected);
            response.Body!.Result.Should().Be(inUse);
        }
    }
}
