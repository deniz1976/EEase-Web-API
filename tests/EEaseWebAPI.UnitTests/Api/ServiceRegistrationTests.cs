using EEaseWebAPI.API.Extensions;
using EEaseWebAPI.Application;
using EEaseWebAPI.Application.Resources;
using EEaseWebAPI.Infrastructure;
using EEaseWebAPI.Persistence;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Xunit;

namespace EEaseWebAPI.UnitTests.Api
{
    public class ServiceRegistrationTests
    {
        private static ServiceProvider BuildContainer()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:PostgreSQL"] = "Host=localhost;Database=eease;Username=eease;Password=eease",
                    ["Token:Issuer"] = "eease-tests",
                    ["Token:Audience"] = "eease-tests",
                    ["Token:SecurityKey"] = "a-signing-key-that-is-long-enough-for-hmac",
                    ["MailService:Enabled"] = "false"
                })
                .Build();

            var environment = Substitute.For<IWebHostEnvironment>();
            environment.EnvironmentName.Returns(Environments.Development);
            environment.ApplicationName.Returns("EEaseWebAPI.API");
            environment.ContentRootPath.Returns(AppContext.BaseDirectory);
            environment.ContentRootFileProvider.Returns(new PhysicalFileProvider(AppContext.BaseDirectory));

            var services = new ServiceCollection();

            services.AddLogging();
            services.AddSingleton(environment);
            services.AddSingleton<IHostEnvironment>(environment);
            services.AddApplicationServices(configuration);
            services.AddInfrastructureServices();
            services.AddPersistenceServices(configuration);

            // Not AddApiServices: that one pulls in MVC and Swagger, whose own internals
            // only resolve under a real web host. What the API layer adds on its own is
            // registered here instead.
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.AddSingleton<GlobalExceptionHandler>();

            return services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });
        }

        [Fact]
        public void Every_registered_service_can_be_built()
        {
            var act = () => BuildContainer().Dispose();

            act.Should().NotThrow();
        }

        [Fact]
        public void The_global_exception_handler_has_everything_it_needs()
        {
            using var container = BuildContainer();

            var resolve = () => container.GetRequiredService<GlobalExceptionHandler>();

            resolve.Should().NotThrow();
        }

        [Fact]
        public void Every_request_handler_can_be_resolved()
        {
            using var container = BuildContainer();
            using var scope = container.CreateScope();

            var handlerTypes = typeof(AppMessages).Assembly.GetTypes()
                .Where(type => !type.IsAbstract && !type.IsInterface)
                .SelectMany(type => type.GetInterfaces()
                    .Where(contract => contract.IsGenericType &&
                                       contract.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
                .ToList();

            handlerTypes.Should().NotBeEmpty();

            foreach (var handlerType in handlerTypes)
            {
                var resolve = () => scope.ServiceProvider.GetService(handlerType);

                resolve.Should().NotThrow($"{handlerType.GenericTypeArguments[0].Name} must be handled");
            }
        }
    }
}
