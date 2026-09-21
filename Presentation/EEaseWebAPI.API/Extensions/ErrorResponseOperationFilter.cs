using EEaseWebAPI.Application.MapEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EEaseWebAPI.API.Extensions
{
    public sealed class ErrorResponseOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var errorSchema = context.SchemaGenerator.GenerateSchema(
                typeof(ErrorResponse), context.SchemaRepository);

            // A request can always be refused for what it carries, either by a validator or
            // by a rule the handler applies.
            Describe(operation, StatusCodes.Status400BadRequest,
                "The request was refused for what it carries.", errorSchema);

            if (RequiresAuthentication(context))
            {
                Describe(operation, StatusCodes.Status401Unauthorized,
                    "The request carried no usable token.", errorSchema);
            }

            // The global limiter covers every endpoint, whether or not it names a policy.
            Describe(operation, StatusCodes.Status429TooManyRequests,
                "Too many requests. The Retry-After header says how long to wait.", errorSchema);

            Describe(operation, StatusCodes.Status500InternalServerError,
                "Something went wrong that the caller cannot do anything about.", errorSchema);

            // Written above the action, because only a handful of endpoints can forbid.
            if (operation.Responses.TryGetValue(
                    StatusCodes.Status403Forbidden.ToString(), out var forbidden))
            {
                forbidden.Description = "The caller may not see or change this.";
                forbidden.Content = JsonOf(errorSchema);
            }
        }

        private static bool RequiresAuthentication(OperationFilterContext context)
        {
            var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;

            return metadata.OfType<IAuthorizeData>().Any()
                && !metadata.OfType<IAllowAnonymous>().Any();
        }

        private static void Describe(
            OpenApiOperation operation, int statusCode, string description, OpenApiSchema schema)
        {
            operation.Responses[statusCode.ToString()] = new OpenApiResponse
            {
                Description = description,
                Content = JsonOf(schema)
            };
        }

        private static Dictionary<string, OpenApiMediaType> JsonOf(OpenApiSchema schema) =>
            new() { ["application/json"] = new OpenApiMediaType { Schema = schema } };
    }
}
