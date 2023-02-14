using Functions.Integration.Test.Framework.Builders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Functions.Integration.Test.Framework.Tests;

public class TestFunctionHostBuilderTest
{
    [Fact]
    public void ShouldBuildUsingPassedInType()
    {
        // arrange
        var builder = TestFunctionHostBuilder.Create<TestStartup>();
        var id = Guid.Empty;

        // apply
        using(var host = builder.Build())
        {
            var services = host.Services;
            var resolver = services.GetRequiredService<TestStartup.TestResolver>();
            id = resolver.Id;
        }

        // assert
        Assert.Equal(TestStartup.Id, id);
    }


    public class TestStartup
    {

        public static Guid Id { get; } = Guid.NewGuid();

        public TestStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<TestResolver>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IApplicationLifetime applicationLifetime, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
        }

        public class TestResolver
        {
            public Guid Id => TestStartup.Id;
        }
    }
}