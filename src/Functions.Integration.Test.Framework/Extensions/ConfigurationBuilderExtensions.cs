using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.WebJobs.Script;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;

namespace Functions.Integration.Test.Framework.Extensions
{
    public static class ConfigurationBuilderExtensions
    {
        public static IWebHostBuilder ConfigureScriptHostServices(this IWebHostBuilder webHostBuilder, Action<IServiceCollection> service) =>
            webHostBuilder.ConfigureServices(s => s.AddSingleton<IConfigureBuilder<IServiceCollection>>(_ => new DelegateActionBuilder<IServiceCollection>(service)));

        public static IWebHostBuilder ConfigureWebJobsBuilder(this IWebHostBuilder webHostBuilder, Action<IWebJobsBuilder> service) =>
            webHostBuilder.ConfigureServices(s => s.AddSingleton<IConfigureBuilder<IWebJobsBuilder>>(_ => new DelegateActionBuilder<IWebJobsBuilder>(service)));


        public class DelegateActionBuilder<TBuilder> : IConfigureBuilder<TBuilder>
        {
            private readonly Action<TBuilder> _builder;

            public DelegateActionBuilder(Action<TBuilder> builder)
            {
                _builder = builder;
            }

            public void Configure(TBuilder builder)
            {
                _builder?.Invoke(builder);
            }
        }
    }
}
