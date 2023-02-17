using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Functions.Integration.Test.Framework.Azure.Functions.FunctionV4;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.WebJobs.Script.WebHost;
using static Microsoft.Azure.WebJobs.Script.EnvironmentSettingNames;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.WebJobs.Script;

namespace Functions.Integration.Test.Framework.Builders
{
    public class TestFunctionHostBuilder
    {
        public IWebHostBuilder? WebHostBuilder { get; private set; }

        public static TestFunctionHostBuilder Create(int port = 7071, string scriptPath = null)
        {

            var hostOptions = new ScriptApplicationHostOptions
            {
                IsSelfHost = true,
                ScriptPath = scriptPath ?? Environment.CurrentDirectory,
                LogPath = Path.Combine(Path.GetTempPath(), "LogFiles", "Application", "Functions"),
                SecretsPath = Path.Combine(Path.GetTempPath(), "secrets", "functions", "secrets")
            };

            var baseUri = new Uri($"http://localhost:{port}");
            var listenUri = new Uri($"http://0.0.0.0:{port}");

            var settings = GetSettings(baseUri, scriptPath);
            UpdateEnvironmentVariablesFromSettings(settings);

            var builder = new TestFunctionHostBuilder();

            var webHostBuilder = WebHost.CreateDefaultBuilder(Array.Empty<string>())
                .ConfigureKestrel(o =>
                {
                    o.Limits.MaxRequestBodySize = 104857600;
                })
                .UseSetting(WebHostDefaults.ApplicationKey, typeof(Startup).Assembly.GetName().Name)
                .UseUrls(listenUri.ToString())
                .ConfigureAppConfiguration(configBuilder =>
                {
                    configBuilder.AddEnvironmentVariables();
                })
                 .ConfigureServices((context, services) =>
                 {
                     services.AddSingleton<IStartup>(new WebJobStartup(context, hostOptions));
                 });

                     builder.WebHostBuilder = webHostBuilder;
            return builder;
        }

        public IWebHost Build()
        {
            return WebHostBuilder.Build();
        }

        private static void UpdateEnvironmentVariablesFromSettings(IDictionary<string, string> settings)
        {
            foreach (var setting in settings)
            {
                if (string.IsNullOrEmpty(setting.Key) || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(setting.Key)))
                {
                    continue;
                }

                Environment.SetEnvironmentVariable(setting.Key, setting.Value);
            }
        }

        private static IDictionary<string, string> GetSettings(Uri listenUri, string scriptPath)
        {
            var settings = new Dictionary<string, string>
            {
                { AzureWebsiteHostName, listenUri.Authority },
                { EnvironmentNameKey , "Development" },
                { AzureWebsiteInstanceId, Guid.NewGuid().ToString("N") },
                { AzureWebJobsScriptRoot, scriptPath },
                { AzureWebsiteHomePath, scriptPath },
            };
            return settings;
        }
    }
}
