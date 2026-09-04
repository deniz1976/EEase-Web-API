using EEaseWebAPI.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace EEaseWebAPI.UnitTests.Infrastructure
{
    public class MailTemplateProviderTests
    {
        [Theory]
        [InlineData("VerificationCode")]
        [InlineData("ResetPassword")]
        [InlineData("DeleteAccount")]
        public void Template_is_read_from_the_embedded_resources(string templateName)
        {
            var rendered = new MailTemplateProvider().Render(templateName, "123456");

            rendered.Should().StartWith("<!DOCTYPE HTML");
            rendered.Should().Contain("</html>");
        }

        [Theory]
        [InlineData("VerificationCode")]
        [InlineData("ResetPassword")]
        [InlineData("DeleteAccount")]
        public void Placeholder_is_replaced_with_the_code(string templateName)
        {
            var rendered = new MailTemplateProvider().Render(templateName, "987654");

            rendered.Should().Contain("987654");
            rendered.Should().NotContain("{{Code}}");
        }

        [Fact]
        public void The_same_template_can_be_rendered_with_different_codes()
        {
            var provider = new MailTemplateProvider();

            var first = provider.Render("VerificationCode", "111111");
            var second = provider.Render("VerificationCode", "222222");

            first.Should().Contain("111111").And.NotContain("222222");
            second.Should().Contain("222222").And.NotContain("111111");
        }

        [Fact]
        public void An_unknown_template_produces_a_clear_error()
        {
            var act = () => new MailTemplateProvider().Render("NoSuchTemplate", "123456");

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*NoSuchTemplate*");
        }
    }
}
