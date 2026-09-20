using System.Reflection;
using EEaseWebAPI.Application.MapEntities;
using FluentAssertions;
using MediatR;
using Xunit;

namespace EEaseWebAPI.UnitTests.Api
{
    /// <summary>
    /// Every endpoint answers with a header and a body. Endpoints used to invent their own
    /// shape, so a caller had to know whether the payload sat under "login", "response",
    /// "userInfo" or straight at the top. Nothing stops the next handler from doing that
    /// again except this.
    /// </summary>
    public class ResponseShapeTests
    {
        public static TheoryData<Type> EveryResponseType()
        {
            var data = new TheoryData<Type>();

            var responses = typeof(ApiResponse<>).Assembly.GetTypes()
                .Where(type => !type.IsAbstract && !type.IsInterface)
                .SelectMany(type => type.GetInterfaces())
                .Where(contract => contract.IsGenericType &&
                                   contract.GetGenericTypeDefinition() == typeof(IRequest<>))
                .Select(contract => contract.GenericTypeArguments[0])
                .Distinct();

            foreach (var response in responses)
            {
                data.Add(response);
            }

            return data;
        }

        [Theory]
        [MemberData(nameof(EveryResponseType))]
        public void Every_response_is_a_header_and_a_body(Type responseType)
        {
            IsApiResponse(responseType).Should().BeTrue(
                $"{responseType.Name} should derive from ApiResponse<TBody>");
        }

        [Theory]
        [MemberData(nameof(EveryResponseType))]
        public void A_response_adds_nothing_of_its_own_next_to_the_header_and_body(Type responseType)
        {
            var declared = responseType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name);

            declared.Should().BeEmpty();
        }

        private static bool IsApiResponse(Type type)
        {
            for (var current = type; current is not null; current = current.BaseType)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(ApiResponse<>))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
