using System.Reflection;
using EEaseWebAPI.API.Constants;
using EEaseWebAPI.API.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using EEaseWebAPI.API.Extensions;
using Xunit;

namespace EEaseWebAPI.UnitTests.Api
{
    /// <summary>
    /// Naming a rate limiting policy that was never registered does not fail at startup: the
    /// endpoint throws on the first request instead, and answers 500 from then on. One
    /// controller shipped like that, so the names are checked here.
    /// </summary>
    public class RateLimitPolicyTests
    {
        public static TheoryData<string, string> NamedPolicies()
        {
            var data = new TheoryData<string, string>();

            foreach (var controller in typeof(ApiControllerBase).Assembly.GetTypes()
                         .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract))
            {
                foreach (var name in PolicyNames(controller))
                {
                    data.Add(controller.Name, name);
                }

                foreach (var action in controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    foreach (var name in PolicyNames(action))
                    {
                        data.Add($"{controller.Name}.{action.Name}", name);
                    }
                }
            }

            return data;
        }

        [Theory]
        [MemberData(nameof(NamedPolicies))]
        public void Every_policy_a_controller_asks_for_is_registered(string endpoint, string policyName)
        {
            var services = new ServiceCollection();
            services.AddApiRateLimiting(new ConfigurationBuilder().Build());

            var options = services.BuildServiceProvider().GetRequiredService<IOptions<RateLimiterOptions>>().Value;

            RegisteredPolicies(options).Should().Contain(
                policyName, $"{endpoint} asks for the '{policyName}' rate limiting policy");
        }

        [Fact]
        public void The_policy_names_the_code_offers_are_all_real()
        {
            var services = new ServiceCollection();
            services.AddApiRateLimiting(new ConfigurationBuilder().Build());

            var options = services.BuildServiceProvider().GetRequiredService<IOptions<RateLimiterOptions>>().Value;

            var registered = RegisteredPolicies(options);

            var offered = typeof(RateLimitPolicies)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Select(field => (string)field.GetRawConstantValue()!);

            offered.Should().BeSubsetOf(registered);
        }

        public static TheoryData<string, string> SecretHandlingEndpoints() => new()
        {
            { "AuthController", "Login" },
            { "AuthController", "RefreshTokenLogin" },
            { "AuthController", "ResetPassword" },
            { "AuthController", "ResetPasswordCodeCheck" },
            { "AuthController", "ResetPasswordWithCode" },
            { "AuthController", "ChangePassword" },
            { "AuthController", "CheckEmailIsInUse" },
            { "UsersController", "CreateUser" },
            { "UsersController", "SendVerificationCodeAgain" },
            { "UsersController", "CheckEmailConfirmed" },
            { "UsersController", "EmailConfirm" },
            { "UsersController", "DeleteAccount" },
            { "UsersController", "DeleteAccountWithCode" }
        };

        [Theory]
        [MemberData(nameof(SecretHandlingEndpoints))]
        public void An_endpoint_that_hands_out_or_checks_a_secret_is_limited(
            string controllerName, string actionName)
        {
            // The global limit allows a hundred requests every thirty seconds, which is a
            // limit on load, not on somebody working through a list of passwords or asking
            // for a hundred reset codes to be mailed to somebody else.
            var controller = typeof(ApiControllerBase).Assembly.GetTypes()
                .Single(type => type.Name == controllerName);

            var action = controller.GetMethods(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Single(method => method.Name == actionName);

            PolicyNames(action).Should().Contain(RateLimitPolicies.Sensitive);
        }

        /// <summary>
        /// The registered policies are not exposed publicly, so they are read off the
        /// options the same way the framework stores them.
        /// </summary>
        private static IEnumerable<string> RegisteredPolicies(RateLimiterOptions options)
        {
            var map = typeof(RateLimiterOptions)
                .GetProperty("PolicyMap", BindingFlags.Instance | BindingFlags.NonPublic);

            map.Should().NotBeNull("the framework should still keep its policies in PolicyMap");

            var policies = map!.GetValue(options) as System.Collections.IDictionary;

            policies.Should().NotBeNull();

            return policies!.Keys.Cast<string>().ToList();
        }

        private static IEnumerable<string> PolicyNames(MemberInfo member) =>
            member.GetCustomAttributes<EnableRateLimitingAttribute>()
                .Select(attribute => attribute.PolicyName)
                .Where(name => !string.IsNullOrEmpty(name))
                .Select(name => name!);
    }
}
