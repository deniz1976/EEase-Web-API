using EEaseWebAPI.Application.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EEaseWebAPI.Infrastructure.Filters
{
    /// <summary>
    /// Turns a model binding failure into the same exception FluentValidation raises, so a
    /// missing field and a field that breaks a rule are reported in one shape rather than
    /// two: the global handler writes both.
    /// </summary>
    public sealed class ValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .ToDictionary(
                        entry => entry.Key,
                        entry => entry.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

                throw new RequestValidationException(errors);
            }

            await next();
        }
    }
}
