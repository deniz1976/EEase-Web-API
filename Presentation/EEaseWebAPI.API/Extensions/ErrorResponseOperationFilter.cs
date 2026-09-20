using EEaseWebAPI.Application.MapEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EEaseWebAPI.API.Extensions
{
    /// <summary>
    /// Says which errors an endpoint can answer with, and with what.
    ///
    /// Every failure in this API is an <see cref="ErrorResponse"/>: the global exception
    /// handler writes one, the authentication events write one, and the rate limiter writes
    /// one. The document did not say so. Fifty-eight actions repeated an attribute for 500,
    /// most repeated one for 401, three mentioned 400 without a type, and one mentioned 429 -
    /// so a generated client saw no shape at all for the error it meets most often.
    ///
    /// The answers are the same for every endpoint of the same kind, so they are derived from
    /// what the endpoint is rather than typed out above it. What differs per endpoint - what
    /// it answers when it works, and the few that can forbid - stays an attribute.
    /// </summary>
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
