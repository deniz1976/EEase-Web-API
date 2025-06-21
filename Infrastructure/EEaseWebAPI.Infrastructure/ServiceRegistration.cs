using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Abstractions.Token;
using EEaseWebAPI.Infrastructure.Services;
using EEaseWebAPI.Infrastructure.Services.Token;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Infrastructure
{
    public static class ServiceRegistration
    {
        public static void AddInfrastructureServices(this IServiceCollection serviceCollection) 
        {
            serviceCollection.AddScoped<ITokenHandler, TokenHandler>();
            serviceCollection.AddScoped<IMailService, MailService>();
            serviceCollection.AddScoped<IHttpService,HttpService>();

        }
    }
}
