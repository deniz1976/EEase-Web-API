using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EEaseWebAPI.API.Extensions
{
    /// <summary>
    /// Swagger writes DateOnly as an object unless it is told otherwise. This is how the
    /// API describes itself, so it lives with the rest of the Swagger setup rather than in
    /// the Application layer, which has no business knowing Swagger exists.
    /// </summary>
    public sealed class DateOnlySchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(DateOnly))
            {
                schema.Type = "string";
                schema.Format = "date";
            }
        }
    }
}
