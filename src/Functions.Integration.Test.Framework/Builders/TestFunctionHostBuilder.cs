using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Functions.Integration.Test.Framework.Azure.Functions.FunctionV4;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Functions.Integration.Test.Framework.Builders
{
    public class TestFunctionHostBuilder
    {
        public IWebHostBuilder? WebHostBuilder { get; private set; }

        public static TestFunctionHostBuilder Create()
        {
            return Create<WebJobStartup>();
        }

        public static TestFunctionHostBuilder Create<TStartup>()
            where TStartup : class
        {
            var builder = new TestFunctionHostBuilder();

            var webHostBuilder = WebHost.CreateDefaultBuilder(Array.Empty<string>())
                .UseStartup<TStartup>();

            builder.WebHostBuilder = webHostBuilder;
            return builder;
        }

        public IWebHost Build()
        {
            return WebHostBuilder.Build();
        }
    }
}
