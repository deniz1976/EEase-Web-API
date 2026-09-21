using System.Reflection;
using Microsoft.AspNetCore.Http;
using EEaseWebAPI.API.Controllers;
using EEaseWebAPI.API.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace EEaseWebAPI.UnitTests.Api
{
    public class ErrorResponseDocumentationTests
    {
        private readonly ErrorResponseOperationFilter _filter = new();

        [Fact]
        public void Anything_can_be_refused_reported_or_throttled()
        {
            var operation = Describe(typeof(UsersController), nameof(UsersController.CreateUser));

            operation.Responses.Keys.Should().Contain(new[] { "400", "429", "500" });
        }

        [Fact]
        public void An_endpoint_behind_a_token_can_answer_that_it_is_missing()
        {
            var operation = Describe(typeof(UsersController), nameof(UsersController.GetUserInfo));

            operation.Responses.Keys.Should().Contain("401");
        }

        [Fact]
        public void An_endpoint_open_to_everybody_does_not_claim_it_can()
        {
            var operation = Describe(typeof(UsersController), nameof(UsersController.CreateUser));

            operation.Responses.Keys.Should().NotContain("401");
        }

        [Theory]
        [InlineData("400")]
        [InlineData("401")]
        [InlineData("429")]
        [InlineData("500")]
        public void Every_error_is_described_as_the_one_shape_they_all_use(string statusCode)
        {
            var operation = Describe(typeof(UsersController), nameof(UsersController.GetUserInfo));

            operation.Responses[statusCode].Content.Should().ContainKey("application/json");
            operation.Responses[statusCode].Content["application/json"].Schema
                .Should().NotBeNull();
        }

        [Fact]
        public void Only_the_endpoints_that_refuse_say_they_can()
        {
            var forbidding = typeof(ApiControllerBase).Assembly.GetTypes()
                .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract)
                .SelectMany(controller => controller
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(action => action.GetCustomAttributes()
                        .OfType<ProducesResponseTypeAttribute>()
                        .Any(attribute => attribute.StatusCode == StatusCodes.Status403Forbidden))
                    .Select(action => $"{controller.Name}.{action.Name}"))
                .OrderBy(name => name);

            forbidding.Should().Equal(
                "PlacesController.DislikePlace",
                "RoutesController.GetMyRouteLike",
                "RoutesController.LikeRoute",
                "RoutesController.UpdateRouteVisibility");
        }

        private OpenApiOperation Describe(Type controller, string actionName)
        {
            var action = controller.GetMethod(actionName)!;

            var descriptor = new ControllerActionDescriptor
            {
                MethodInfo = action,
                ControllerTypeInfo = controller.GetTypeInfo(),
                EndpointMetadata = action.GetCustomAttributes(inherit: true)
                    .Concat(controller.GetCustomAttributes(inherit: true))
                    .OfType<object>()
                    .ToList()
            };

            var operation = new OpenApiOperation
            {
                Responses = new OpenApiResponses
                {
                    ["200"] = new OpenApiResponse { Description = "Success" }
                }
            };

            _filter.Apply(operation, Context(descriptor, action));

            return operation;
        }

        private static OperationFilterContext Context(ActionDescriptor descriptor, MethodInfo action) =>
            new(
                new ApiDescription { ActionDescriptor = descriptor },
                new SchemaGenerator(new SchemaGeneratorOptions(), new JsonSerializerDataContractResolver(new())),
                new SchemaRepository(),
                action);
    }
}
