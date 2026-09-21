using System.Reflection;
using EEaseWebAPI.API.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace EEaseWebAPI.UnitTests.Api
{
    public class ControllerCancellationTests
    {
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
        public void Every_action_that_waits_can_be_cancelled(string controllerName, string actionName)
        {
            var actions = Actions()
                .Where(entry => entry.Controller.Name == controllerName && entry.Action.Name == actionName)
                .Select(entry => entry.Action);

            actions.Should().OnlyContain(
                action => action.GetParameters()
                    .Any(parameter => parameter.ParameterType == typeof(CancellationToken)),
                $"{controllerName}.{actionName} should take the token the request is aborted with");
        }

        private static IEnumerable<(Type Controller, MethodInfo Action)> Actions() =>
            typeof(ApiControllerBase).Assembly.GetTypes()
                .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract)
                .SelectMany(controller => controller
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(action => typeof(Task).IsAssignableFrom(action.ReturnType))
                    .Select(action => (controller, action)));
    }
}
