using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.GetCitiesBySearch;
using EEaseWebAPI.Application.MapEntities;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Net.Mime;
using System.Text.Json;

namespace EEaseWebAPI.API.Extensions
{
    static public class ConfigureExceptionHandlerExtension
    {
        public static void ConfigureExceptionHandler<T>(this WebApplication application, ILogger<T> logger)
        {
            application.UseExceptionHandler(builder =>
            {
                builder.Run(async context =>
                {
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        context.Response.ContentType = MediaTypeNames.Application.Json;

                        int? statusenum = (int)StatusEnum.UnknownError;
                        var exception = contextFeature.Error;

                        if (exception is InvalidSearchTermException)
                            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        else if (exception is UnauthorizedAccessException)
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        else if (exception is ArgumentNullException || exception is ArgumentException)
                            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        else
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                        if (exception is BaseException baseException)
                        {
                            statusenum = baseException.EnumStatusCode;
                        }
                        

                        logger.LogError($"Error: {exception.Message}");

                        await context.Response.WriteAsync(JsonSerializer.Serialize(new GlobalError
                        {
                            StatusCode = context.Response.StatusCode,
                            Message = exception.Message,
                            Title = "Error",
                            EnumStatusCode = statusenum
                        }));
                    }
                });
            });
        }
    }
}
