using System.Diagnostics;
using System.Net;
using Autofac.Core;
using Functions.Integration.Test.Framework.Builders;
using Functions.Integration.Test.Framework.Extensions;
using Functions.Integration.Test.Framework.Loggers;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Script;
using Microsoft.Azure.WebJobs.Script.WebHost;
using Microsoft.Azure.WebJobs.Script.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Xunit.Abstractions;

namespace Functions.Integration.Test.Framework.Tests;

public class TestFunctionHostBuilderTest
{
    private readonly ITestOutputHelper outputHelper;

    public TestFunctionHostBuilderTest(ITestOutputHelper outputHelper)
    {
        this.outputHelper = outputHelper;
    }


    [Fact]
    public void ShouldBuildUsingPassedInType()
    {
        // arrange
        var builder = TestFunctionHostBuilder.Create();
        builder.WebHostBuilder.UseStartup<TestStartup>();

        var id = Guid.Empty;

        // apply
        using (var host = builder.Build())
        {
            var services = host.Services;
            var resolver = services.GetRequiredService<TestStartup.TestResolver>();
            id = resolver.Id;
        }

        // assert
        Assert.Equal(TestStartup.Id, id);
    }

    [Fact]
    public async Task ShouldRunBasicFunction()
    {
        // arrange
        const string EXPECTED = "Hello, test. This HTTP triggered function executed successfully.";
        string path = Path.GetDirectoryName(Path.GetFullPath(typeof(TestStartup).Assembly.Location));
        string workersPath = Path.Combine(path, WorkerConstants.DefaultWorkersDirectoryName);
        if (!Directory.Exists(workersPath))
        {
            Directory.CreateDirectory(workersPath);
        }

        string scriptPath = Path.GetFullPath(Path.Combine(path, "..", "..", "..", "..", "..", "src", "Functions.Integration.Test.Sample", "bin", "Debug", "net6.0"));

        var builder = TestFunctionHostBuilder.Create(scriptPath: scriptPath);
        builder.WebHostBuilder.ConfigureLogging(l =>
        {
            l.AddProvider(new TestOutputLoggerProvider(outputHelper));
        });

        // apply
        var server = new TestServer(builder.WebHostBuilder);
        server.BaseAddress = new Uri("http://localhost/");

        var manager = server.Host.Services.GetService<IScriptHostManager>();
        await manager.DelayUntilHostReady();

        var client = server.CreateClient();

        await TaskBuilder
            .While(() => client.IsHostRunningAsync())
            .Poll()
            .UntilTimeout()
            .Go();

        // assert
        var response = await client.GetAsync("api/BasicFunction?name=test");
        var str = await response.Content.ReadAsStringAsync();
        Assert.Equal(EXPECTED, str);
    }


    [Fact]
    public async Task ShouldRunOrchestrationFunction()
    {
        // arrange
        string path = Path.GetDirectoryName(Path.GetFullPath(typeof(TestStartup).Assembly.Location));
        string workersPath = Path.Combine(path, WorkerConstants.DefaultWorkersDirectoryName);
        if (!Directory.Exists(workersPath))
        {
            Directory.CreateDirectory(workersPath);
        }

        string scriptPath = Path.GetFullPath(Path.Combine(path, "..", "..", "..", "..", "..", @"src\Functions.Integration.Test.Sample\bin\Debug\net6.0\"));

        var builder = TestFunctionHostBuilder.Create(scriptPath: scriptPath);
        builder.WebHostBuilder.ConfigureLogging(l =>
        {
            l.AddProvider(new TestOutputLoggerProvider(outputHelper));
        });

        // apply
        var server = new TestServer(builder.WebHostBuilder);
        server.BaseAddress = new Uri("http://localhost/");

        var manager = server.Host.Services.GetService<IScriptHostManager>();
        await manager.DelayUntilHostReady();

        var client = server.CreateClient();

        await TaskBuilder
            .While(() => client.IsHostRunningAsync())
            .Poll()
            .UntilTimeout()
            .Go();


        var response = await client.GetAsync("api/OrchestrationFunction_HttpStart");

        string responseUrl = response.Headers.Location.ToString();

        await TaskBuilder
            .While(async () =>
            {
                response = await client.GetAsync(responseUrl);
                var isAccepted = (response.StatusCode == HttpStatusCode.Accepted);
                return !isAccepted;
            })
            .Poll()
            .UntilTimeout()
            .Go();

        // assert
        dynamic responseObj = await response.Content.ReadAsAsync<dynamic>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        JArray output = (JArray)responseObj.output;
        var list = output.ToObject<string[]>();

        Assert.Equal(new string[]
        {
            "Hello Tokyo!", 
            "Hello Seattle!",
            "Hello London!",
        }, list);

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