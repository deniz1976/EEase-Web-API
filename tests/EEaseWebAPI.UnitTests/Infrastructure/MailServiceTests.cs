using EEaseWebAPI.Application.Options;
using EEaseWebAPI.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EEaseWebAPI.UnitTests.Infrastructure
{
    public class MailServiceTests
    {
        private static MailService For(MailOptions options) =>
            new(Options.Create(options), new MailTemplateProvider(), NullLogger<MailService>.Instance);

        private static MailOptions Configured(bool enabled = true) => new()
        {
            Enabled = enabled,
            Email = "eease@example.com",
            Key = "a-key",
            Name = "EEase",
            // Nothing listens here, so connecting fails rather than hanging.
            Host = "127.0.0.1",
            Port = 1,
            TimeoutSeconds = 5
        };

        [Fact]
        public async Task Nothing_is_sent_while_delivery_is_switched_off()
        {
            var service = For(Configured(enabled: false));

            await service.SendEmailAsync("ada@example.com", "Subject", "<p>Body</p>");
        }

        [Fact]
        public async Task A_missing_credential_is_reported_rather_than_attempted()
        {
            var service = For(new MailOptions { Enabled = true, Email = string.Empty, Key = string.Empty });

            var sending = () => service.SendEmailAsync("ada@example.com", "Subject", "<p>Body</p>");

            await sending.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*MailService:Email*");
        }

        [Theory]
        [InlineData("verification")]
        [InlineData("reset")]
        [InlineData("delete")]
        public async Task A_code_that_could_not_be_sent_is_answered_with_false(string kind)
        {
            var service = For(Configured());

            var sent = kind switch
            {
                "verification" => await service.SendVerificationEmailAsync("ada@example.com", "Subject", "123456"),
                "reset" => await service.SendResetPasswordEmailAsync("ada@example.com", "Subject", "123456"),
                _ => await service.SendDeleteCodeEmailAsync("ada@example.com", "Subject", "123456")
            };

            sent.Should().BeFalse("nothing is listening on the host these options name");
        }

        [Fact]
        public async Task A_code_counts_as_sent_when_delivery_is_switched_off()
        {
            var service = For(Configured(enabled: false));

            (await service.SendVerificationEmailAsync("ada@example.com", "Subject", "123456"))
                .Should().BeTrue();
        }

        [Fact]
        public async Task A_caller_who_gave_up_is_not_reported_as_a_delivery_failure()
        {
            var service = For(Configured());

            using var cancelled = new CancellationTokenSource();
            await cancelled.CancelAsync();

            var sending = () => service.SendVerificationEmailAsync(
                "ada@example.com", "Subject", "123456", cancelled.Token);

            await sending.Should().ThrowAsync<OperationCanceledException>();
        }
    }
}
