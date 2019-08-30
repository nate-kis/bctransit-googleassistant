using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using NateK.BCTransit;
using NateK.Lib;
using NextBusFncApp.Classes;

[assembly: FunctionsStartup(typeof(NextBusFncApp.Startup))]

namespace NextBusFncApp
{

    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // set config

            var config = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            builder.Services.AddSingleton(config);


            // set services
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<IHttpService, HttpService>();

            builder.Services.AddScoped<IBCTransitRouteSchedule, BCTransitRouteSchedule>();

        }
    }
}
