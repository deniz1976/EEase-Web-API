using EEaseWebAPI.Infrastructure.Services;
using FluentAssertions;
using Xunit;
using EEaseWebAPI.Application;
using EEaseWebAPI.UnitTests.Localization;

namespace EEaseWebAPI.UnitTests.Infrastructure
{
    public class MailTemplateProviderTests
    {
        [Theory]
        [InlineData(MailTemplate.VerificationCode)]
        [InlineData(MailTemplate.ResetPassword)]
        [InlineData(MailTemplate.DeleteAccount)]
        public void Template_is_read_from_the_embedded_resources(MailTemplate templateName)
        {
            var rendered = new MailTemplateProvider().Render(templateName, "123456");

            rendered.Should().StartWith("<!DOCTYPE HTML");
            rendered.Should().Contain("</html>");
        }

        [Theory]
        [InlineData(MailTemplate.VerificationCode)]
        [InlineData(MailTemplate.ResetPassword)]
        [InlineData(MailTemplate.DeleteAccount)]
        public void Placeholder_is_replaced_with_the_code(MailTemplate templateName)
        {
            var rendered = new MailTemplateProvider().Render(templateName, "987654");

            rendered.Should().Contain("987654");
            rendered.Should().NotContain("{{Code}}");
        }

        [Fact]
        public void The_same_template_can_be_rendered_with_different_codes()
        {
            var provider = new MailTemplateProvider();

            var first = provider.Render(MailTemplate.VerificationCode, "111111");
            var second = provider.Render(MailTemplate.VerificationCode, "222222");

            first.Should().Contain("111111").And.NotContain("222222");
            second.Should().Contain("222222").And.NotContain("111111");
        }

        [Theory]
        [InlineData(MailTemplate.VerificationCode)]
        [InlineData(MailTemplate.ResetPassword)]
        [InlineData(MailTemplate.DeleteAccount)]
        public void Nothing_is_left_unfilled_in_the_rendered_mail(MailTemplate templateName)
        {
            var rendered = new MailTemplateProvider().Render(templateName, "123456");

            rendered.Should().NotContain("{{");
        }

        [Fact]
        public void The_mail_is_written_in_the_language_of_the_recipient()
        {
            var provider = new MailTemplateProvider();

            var english = Culture.Use("en", () => provider.Render(MailTemplate.VerificationCode, "123456"));
            var turkish = Culture.Use("tr", () => provider.Render(MailTemplate.VerificationCode, "123456"));

            english.Should().Contain("Your Verification Code");
            turkish.Should().Contain("Doğrulama Kodunuz");
            turkish.Should().NotContain("Your Verification Code");
        }

        [Fact]
        public void An_unknown_template_produces_a_clear_error()
        {
            var act = () => new MailTemplateProvider().Render((MailTemplate)99, "123456");

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*99*");
        }
    }
}
