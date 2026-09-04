using EEaseWebAPI.API.Constants;
using EEaseWebAPI.API.Extensions;
using EEaseWebAPI.Application;
using EEaseWebAPI.Infrastructure;
using EEaseWebAPI.Persistence;
using EEaseWebAPI.Persistence.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseExceptionHandler();

app.UseSwaggerDocumentation();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseCors(CorsPolicies.Default);
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Redirect("/swagger/index.html")).ExcludeFromDescription();

await app.MigrateDatabaseAsync();

await app.RunAsync();

public partial class Program;
