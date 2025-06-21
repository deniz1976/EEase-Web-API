using EEaseWebAPI.API.Extensions;
using EEaseWebAPI.Application;
using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Features.Commands.AppUser.CreateUser;
using EEaseWebAPI.Application.JsonConverters;
using EEaseWebAPI.Infrastructure;
using EEaseWebAPI.Infrastructure.Filters;
using EEaseWebAPI.Persistence;
using EEaseWebAPI.Persistence.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: "global_key",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromSeconds(30),
                QueueLimit = 0
            });
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
        }

        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            Error = "Rate limit exceeded. Too many requests.",
            RetryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry) 
                ? (int)retry.TotalSeconds 
                : 30,
            Details = "Maximum allowed: 100 requests per 30 seconds."
        }, cancellationToken);
    };
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
.AllowAnyHeader()
.AllowAnyMethod()
.AllowAnyOrigin()));


//builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>())
//    .AddFluentValidation(configuration => configuration.RegisterValidatorsFromAssemblyContaining<CreateUserCommandRequest>())
//    .ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);

builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>())
    .ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserCommandRequest>();

builder.Services.AddSingleton<IGeminiKeyManager, GeminiKeyManager>();
builder.Services.AddHttpClient<IGeminiAIService, GeminiAIService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer("User", options => options.TokenValidationParameters = new()
{
    ValidateAudience = true,
    ValidateIssuer = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,

    ValidAudience = builder.Configuration["Token:Audience"],
    ValidIssuer = builder.Configuration["Token:Issuer"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Token:SecurityKey"])), 
    LifetimeValidator = (notBefore,expires,securityToken,validationParameters) => expires != null ? expires > DateTime.UtcNow : false,
    NameClaimType = ClaimTypes.Name

}) ;

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.MaxDepth = 64;
    });

builder.Services.AddSwaggerGen(options =>
{
    options.SchemaFilter<DateOnlySchemaFilter>();
});

var app = builder.Build();

app.UseRateLimiter();

app.UseSwagger();
app.UseSwaggerUI();

app.ConfigureExceptionHandler<Program>(app.Services.GetRequiredService<ILogger<Program>>());
app.UseCors();
app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapGet("/", context =>
    {
        context.Response.Redirect("/swagger/index.html");
        return Task.CompletedTask; 
    });
});


app.Run();
