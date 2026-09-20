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
    /// The API used to route by action name, so a C# method name was the URL and every
    /// endpoint read "api/Users/GetUserInfo": the verb said twice, the resource said twice,
    /// and the casing whatever the method happened to use. Paths are written out now, and
    /// these are the rules they are written to.
    /// </summary>
    public class EndpointConventionTests
    {
        /// <summary>
        /// A path names things, and the HTTP method says what is being done to them. These
        /// are the segments that are not a thing, each one a decision somebody wrote down:
        /// the four under auth, because there is no noun for proving who you are; the two
        /// that describe which part of a collection is wanted; and the four that really
        /// are nouns while reading like verbs. Adding one is meant to be a decision, which
        /// is why they are written here rather than inferred.
        /// </summary>
        private static readonly HashSet<string> NotNouns = new()
        {
            "login",
            "refresh",
            "verify",
            "complete",
            "recommended",
            "liked",
            "likes",
            "dislikes",
            "blocked-users",
            "deletion-request"
        };

        private static readonly string[] VerbPrefixes =
        {
            "get", "create", "update", "delete", "check", "set", "search", "send",
            "remove", "cancel", "block", "unblock", "like", "dislike", "respond",
            "reset", "confirm", "login", "logout", "refresh", "verify", "complete"
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
        public void A_path_is_written_out_in_lower_case(string controllerName, string actionName)
        {
            foreach (var path in Paths(controllerName, actionName))
            {
                path.Should().NotContain("[", "a path is written out, not derived from a class or method name");

                // A placeholder is a C# parameter name and a route constraint; only the
                // part a caller types has to be lower case.
                var written = string.Join('/', path.Split('/')
                    .Where(segment => !segment.StartsWith('{')));

                written.Should().Be(written.ToLowerInvariant(),
                    $"{controllerName}.{actionName} answers a URL a person has to type");
            }
        }

        [Theory]
        [MemberData(nameof(EveryAction))]
        public void A_path_starts_at_the_api(string controllerName, string actionName)
        {
            foreach (var path in Paths(controllerName, actionName))
            {
                path.Should().StartWith("api/", $"{controllerName}.{actionName}");
            }
        }

        [Theory]
        [MemberData(nameof(EveryAction))]
        public void A_path_names_things_rather_than_what_is_done_to_them(
            string controllerName, string actionName)
        {
            foreach (var path in Paths(controllerName, actionName))
            {
                var segments = path.Split('/')
                    .Where(segment => segment.Length > 0 && !segment.StartsWith('{'))
                    .Where(segment => segment != "api");

                foreach (var segment in segments)
                {
                    if (NotNouns.Contains(segment))
                    {
                        continue;
                    }

                    VerbPrefixes.Should().NotContain(
                        prefix => segment.StartsWith(prefix, StringComparison.Ordinal),
                        $"'{segment}' in {path} reads as something being done, and the " +
                        "HTTP method already says that");
                }
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
        public void No_two_endpoints_answer_the_same_verb_and_path()
        {
            var endpoints = Actions()
                .SelectMany(entry => Paths(entry.Controller.Name, entry.Action.Name)
                    .SelectMany(path => Verbs(entry.Action).Select(verb => $"{verb} {path}")))
                .ToList();

            endpoints.Should().OnlyHaveUniqueItems();
        }

        private static IEnumerable<string> Verbs(MethodInfo action) =>
            action.GetCustomAttributes()
                .OfType<HttpMethodAttribute>()
                .SelectMany(attribute => attribute.HttpMethods);

        /// <summary>
        /// The full paths an action answers on: the controller's prefix and the action's
        /// own template, unless the action's template starts with a slash and stands alone.
        /// </summary>
        private static IEnumerable<string> Paths(string controllerName, string actionName)
        {
            var entry = Actions()
                .Single(candidate => candidate.Controller.Name == controllerName &&
                                     candidate.Action.Name == actionName);

            var prefix = entry.Controller.GetCustomAttributes()
                .OfType<RouteAttribute>()
                .Select(attribute => attribute.Template)
                .FirstOrDefault() ?? string.Empty;

            var templates = entry.Action.GetCustomAttributes()
                .OfType<HttpMethodAttribute>()
                .Select(attribute => attribute.Template)
                .ToList();

            if (templates.Count == 0 || templates.All(template => template is null))
            {
                return new[] { prefix };
            }

            return templates
                .Where(template => template is not null)
                .Select(template => template!.StartsWith('/')
                    ? template.TrimStart('/')
                    : $"{prefix}/{template}")
                .Distinct();
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
