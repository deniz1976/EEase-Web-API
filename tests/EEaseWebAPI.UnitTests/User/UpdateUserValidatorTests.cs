using EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUser;
using EEaseWebAPI.Application.Validators.User;
using EEaseWebAPI.UnitTests.Localization;
using FluentAssertions;
using Xunit;

namespace EEaseWebAPI.UnitTests.User
{
    public class UpdateUserValidatorTests
    {
        private static UpdateUserValidator Build(string culture = "en") =>
            Culture.Use(culture, () => new UpdateUserValidator());

        private static UpdateUserCommandRequest Request(
            string? username = null,
            string? name = null,
            string? surname = null,
            string? gender = null,
            string? bio = null,
            DateOnly? bornDate = null) =>
            new()
            {
                User = "alice",
                Username = username,
                Name = name,
                Surname = surname,
                Gender = gender,
                Bio = bio,
                BornDate = bornDate
            };

        [Fact]
        public void An_update_that_changes_nothing_is_valid()
        {
            Build().Validate(Request()).IsValid.Should().BeTrue();
        }

        [Fact]
        public void An_empty_bio_is_allowed()
        {
            Build().Validate(Request(bio: "")).IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("ab")]
        [InlineData("alice bond")]
        public void A_username_that_breaks_the_rules_is_refused(string username)
        {
            Build().Validate(Request(username: username)).IsValid.Should().BeFalse();
        }

        [Fact]
        public void A_name_longer_than_the_limit_is_refused_with_the_limit_in_the_message()
        {
            var result = Build().Validate(Request(name: new string('a', UserProfileRules.NameMaxLength + 1)));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.ErrorMessage.Should().Contain(UserProfileRules.NameMaxLength.ToString());
        }

        [Fact]
        public void A_one_letter_surname_is_refused()
        {
            Build().Validate(Request(surname: "A")).IsValid.Should().BeFalse();
        }

        [Fact]
        public void A_bio_over_the_limit_is_refused()
        {
            var bio = new string('x', UserProfileRules.BioMaxLength + 1);

            Build().Validate(Request(bio: bio)).IsValid.Should().BeFalse();
        }

        [Fact]
        public void An_unknown_gender_is_refused()
        {
            Build().Validate(Request(gender: "Other")).IsValid.Should().BeFalse();
        }

        [Fact]
        public void A_birth_date_that_makes_the_user_too_young_is_refused()
        {
            var bornDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-UserProfileRules.MinimumAge).AddDays(1);

            Build().Validate(Request(bornDate: bornDate)).IsValid.Should().BeFalse();
        }

        [Fact]
        public void Turning_the_minimum_age_today_is_allowed()
        {
            var bornDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-UserProfileRules.MinimumAge);

            Build().Validate(Request(bornDate: bornDate)).IsValid.Should().BeTrue();
        }

        [Fact]
        public void The_caller_is_answered_in_their_own_language()
        {
            var result = Build("tr").Validate(Request(gender: "Other"));

            result.Errors.Should().ContainSingle()
                .Which.ErrorMessage.Should().Be("Cinsiyet 'Male' veya 'Female' olmalıdır.");
        }
    }
}
