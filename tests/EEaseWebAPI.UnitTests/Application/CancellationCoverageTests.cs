using System.Reflection;
using System.Threading;
using EEaseWebAPI.Application.Resources;
using FluentAssertions;
using Xunit;

namespace EEaseWebAPI.UnitTests.Application
{
    /// <summary>
    /// Work that a caller has walked away from should stop. A service method that cannot be
    /// handed a token is a place where it cannot, and the token then stops at whoever calls
    /// it, so the rule is checked rather than remembered.
    /// </summary>
    public class CancellationCoverageTests
    {
        /// <summary>Nothing to cancel: these build or format a value in memory.</summary>
        private static readonly HashSet<string> NothingToCancel = new()
        {
            "IHeaderService",
            "IPlaceQueryBuilder",
            "IPreferenceProfileBuilder",
            "IRoutePlanValidator",
            "IVerificationCodeGenerator"
        };

        public static TheoryData<string, string> EveryServiceMethod()
        {
            var data = new TheoryData<string, string>();

            var contracts = typeof(AppMessages).Assembly.GetTypes()
                .Where(type => type.IsInterface && type.Namespace is not null)
                .Where(type => type.Namespace!.StartsWith("EEaseWebAPI.Application.Abstractions.Services"))
                .Where(type => !NothingToCancel.Contains(type.Name));

            foreach (var contract in contracts)
            {
                foreach (var method in contract.GetMethods())
                {
                    if (typeof(Task).IsAssignableFrom(method.ReturnType))
                    {
                        data.Add(contract.Name, method.Name);
                    }
                }
            }

            return data;
        }

        [Theory]
        [MemberData(nameof(EveryServiceMethod))]
        public void Every_service_method_that_waits_can_be_cancelled(string contractName, string methodName)
        {
            var contract = typeof(AppMessages).Assembly.GetTypes()
                .Single(type => type.IsInterface && type.Name == contractName);

            var methods = contract.GetMethods().Where(method => method.Name == methodName);

            methods.Should().OnlyContain(
                method => method.GetParameters()
                    .Any(parameter => parameter.ParameterType == typeof(CancellationToken)),
                $"{contractName}.{methodName} should take a cancellation token");
        }
    }
}
