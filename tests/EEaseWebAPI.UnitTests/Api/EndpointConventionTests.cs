using System.Reflection;
using EEaseWebAPI.API.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Xunit;

namespace EEaseWebAPI.UnitTests.Api
{
    /// <summary>
    /// The controllers route by action name, so the name of a method is the URL a caller
    /// types and the verb is the promise made about it. Both used to drift: a photo was
    /// fetched with POST, an account was deleted with POST, two different endpoints were both
    /// called GetAllRoutes, and one name carried an Async suffix into the URL.
    /// </summary>
    public class EndpointConventionTests
    {
        /// <summary>
        /// What a name starting with each of these promises. A name outside the table is
        /// free to use whichever verb suits it, such as Login or RespondToFriendRequest.
        /// </summary>
        private static readonly (string Prefix, Type Verb)[] Promises =
        {
            ("Get", typeof(HttpGetAttribute)),
            ("Check", typeof(HttpGetAttribute)),
            ("Search", typeof(HttpGetAttribute)),
            ("Create", typeof(HttpPostAttribute)),
            ("Update", typeof(HttpPutAttribute)),
            ("Set", typeof(HttpPutAttribute)),
            ("Delete", typeof(HttpDeleteAttribute)),
            ("Remove", typeof(HttpDeleteAttribute)),
            ("Cancel", typeof(HttpDeleteAttribute)),
            ("Unblock", typeof(HttpDeleteAttribute))
        };

        public static TheoryData<string, string> EveryAction()
        {
            var data = new TheoryData<string, string>();

            foreach (var (controller, action) in Actions())
            {
                data.Add(controller.Name, action.Name);
            }

            return data;
        }

        [Theory]
        [MemberData(nameof(EveryAction))]
        public void An_action_name_does_not_carry_a_suffix_into_the_url(string controllerName, string actionName)
        {
            _ = controllerName;

            actionName.Should().NotEndWith(
                "Async", "the action name is the URL, and no caller should have to type it");
        }

        [Theory]
        [MemberData(nameof(EveryAction))]
        public void An_action_uses_the_verb_its_name_promises(string controllerName, string actionName)
        {
            var action = Action(controllerName, actionName);

            foreach (var (prefix, verb) in Promises)
            {
                if (!actionName.StartsWith(prefix, StringComparison.Ordinal))
                {
                    continue;
                }

                action.GetCustomAttributes()
                    .Select(attribute => attribute.GetType())
                    .Should().Contain(verb,
                        $"{controllerName}.{actionName} starts with '{prefix}'");

                return;
            }
        }

        [Theory]
        [MemberData(nameof(EveryAction))]
        public void Every_parameter_says_where_it_is_bound_from(string controllerName, string actionName)
        {
            var parameters = Action(controllerName, actionName).GetParameters()
                .Where(parameter => parameter.ParameterType != typeof(CancellationToken));

            foreach (var parameter in parameters)
            {
                parameter.GetCustomAttributes()
                    .OfType<IBindingSourceMetadata>()
                    .Should().NotBeEmpty(
                        $"{controllerName}.{actionName} takes '{parameter.Name}' from somewhere " +
                        "the reader should not have to infer");
            }
        }

        [Theory]
        [MemberData(nameof(EveryAction))]
        public void A_read_does_not_ask_for_a_body(string controllerName, string actionName)
        {
            var action = Action(controllerName, actionName);

            if (!action.GetCustomAttributes().Any(attribute => attribute is HttpGetAttribute))
            {
                return;
            }

            action.GetParameters()
                .SelectMany(parameter => parameter.GetCustomAttributes())
                .Should().NotContain(attribute => attribute is FromBodyAttribute,
                    $"{controllerName}.{actionName} answers a GET");
        }

        [Fact]
        public void No_two_endpoints_on_a_controller_share_a_name()
        {
            foreach (var controller in Actions().Select(entry => entry.Controller).Distinct())
            {
                var names = Actions()
                    .Where(entry => entry.Controller == controller)
                    .Select(entry => entry.Action.Name);

                names.Should().OnlyHaveUniqueItems(
                    $"{controller.Name} routes by action name, so a repeat is two URLs with one name");
            }
        }

        private static MethodInfo Action(string controllerName, string actionName) =>
            Actions()
                .Single(entry => entry.Controller.Name == controllerName && entry.Action.Name == actionName)
                .Action;

        private static IEnumerable<(Type Controller, MethodInfo Action)> Actions() =>
            typeof(ApiControllerBase).Assembly.GetTypes()
                .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract)
                .SelectMany(controller => controller
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(action => typeof(Task).IsAssignableFrom(action.ReturnType))
                    .Select(action => (controller, action)));
    }
}
