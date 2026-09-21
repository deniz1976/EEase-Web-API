using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Persistence.Services;
using FluentAssertions;
using Xunit;

namespace EEaseWebAPI.UnitTests.Api
{
    public class HeaderServiceTests
    {
        private readonly HeaderService _service = new();

        [Fact]
        public void A_header_carries_the_code_it_was_asked_for()
        {
            var header = _service.HeaderCreate((int)StatusEnum.RouteCreatedSuccessfully);

            header.EnumStatusCode.Should().Be((int)StatusEnum.RouteCreatedSuccessfully);
            header.Success.Should().BeTrue();
        }

        [Fact]
        public void A_header_can_say_the_request_did_not_work()
        {
            _service.HeaderCreate((int)StatusEnum.UserNotFound, success: false)
                .Success.Should().BeFalse();
        }

        [Fact]
        public void The_time_is_read_in_utc_so_that_two_servers_agree()
        {
            var before = DateTime.UtcNow.AddSeconds(-1);

            var header = _service.HeaderCreate();

            header.ResponseDate.Should().BeOnOrAfter(before);
            header.ResponseDate.Should().BeOnOrBefore(DateTime.UtcNow.AddSeconds(1));
        }

        [Fact]
        public void A_caller_that_knows_the_time_is_believed()
        {
            var stamped = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc);

            _service.HeaderCreate(responseDate: stamped).ResponseDate.Should().Be(stamped);
        }
    }
}
