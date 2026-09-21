using EEaseWebAPI.Application.Resources;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Xunit;

namespace EEaseWebAPI.UnitTests.Application
{
    public class ValidatorCoverageTests
    {
        private static readonly HashSet<string> CarryNothingToValidate = new()
        {
            "CheckRouteLikeStatusQueryRequest",
            "DeleteAllRoutesCommandRequest",
            "DeleteRouteCommandRequest",
            "DeleteUserCommandRequest",
            "GetAllCountriesQueryRequest",
            "GetAllRoutesQueryRequest",
            "GetAllTopicsQueryRequest",
            "GetBlockedUsersQuery",
            "SearchCitiesQueryRequest",
            "GetCurrenciesQueryRequest",
            "GetLikedRoutesQueryRequest",
            "GetPendingFriendRequestsQuery",
            "GetRouteByIdQueryRequest",
            "GetPlacePhotoQueryRequest",
            "GetRoutesByUserIdQueryRequest",
            "GetUserCurrencyQueryRequest",
            "GetUserFriendsQuery",
            "GetUserInfoQueryRequest",
            "GetUserPhotoQueryRequest",
            "GetUserPreferenceDescriptionsQueryRequest",
            "LikeRouteCommandRequest",
            "LoginUserCommandRequest",
            "ResetUserPreferencesCommandRequest",
            "SearchUsersQueryRequest",
            "GetAccountStatusQueryRequest",
            "UpdateRouteStatusCommandRequest",
            "UpdateUserCountryCommandRequest",
            "UpdateUserCurrencyCommandRequest",
            "UpdateUserPreferencesCommandRequest",
            "UpdateUserPreferencesWithTopicsCommandRequest"
        };

        [Fact]
        public void Every_request_either_has_a_validator_or_is_listed_as_needing_none()
        {
            var assembly = typeof(AppMessages).Assembly;

            var validated = assembly.GetTypes()
                .Where(type => !type.IsAbstract && !type.IsInterface)
                .SelectMany(type => type.GetInterfaces())
                .Where(contract => contract.IsGenericType &&
                                   contract.GetGenericTypeDefinition() == typeof(IValidator<>))
                .Select(contract => contract.GenericTypeArguments[0])
                .ToHashSet();

            var requests = assembly.GetTypes()
                .Where(type => !type.IsAbstract && !type.IsInterface)
                .Where(type => type.GetInterfaces().Any(contract =>
                    contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IRequest<>)))
                .ToList();

            requests.Should().NotBeEmpty();

            var unvalidated = requests
                .Where(request => !validated.Contains(request))
                .Select(request => request.Name)
                .Where(name => !CarryNothingToValidate.Contains(name))
                .OrderBy(name => name)
                .ToList();

            unvalidated.Should().BeEmpty();
        }

        [Fact]
        public void The_list_of_requests_that_need_no_validator_has_no_leftovers()
        {
            var assembly = typeof(AppMessages).Assembly;

            var requestNames = assembly.GetTypes()
                .Where(type => type.GetInterfaces().Any(contract =>
                    contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IRequest<>)))
                .Select(type => type.Name)
                .ToHashSet();

            CarryNothingToValidate.Where(name => !requestNames.Contains(name)).Should().BeEmpty();
        }
    }
}
