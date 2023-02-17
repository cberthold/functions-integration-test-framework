using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.WebJobs.Script;
using Microsoft.Azure.WebJobs.Script.WebHost;
using Microsoft.Azure.WebJobs.Script.WebHost.Authentication;
using Microsoft.Azure.WebJobs.Script.WebHost.Controllers;
using Microsoft.Azure.WebJobs.Script.WebHost.DependencyInjection;
using Microsoft.Azure.WebJobs.Script.WebHost.Security.Authentication;
using Microsoft.Azure.WebJobs.Script.WebHost.Security;
using Microsoft.Extensions.DependencyInjection;

namespace Functions.Integration.Test.Framework.Azure.Functions
{
    namespace FunctionV4
    {
        public class WebJobStartup : IStartup
        {
            private readonly WebHostBuilderContext builderContext;
            private readonly ScriptApplicationHostOptions hostOptions;

            public WebJobStartup(
                WebHostBuilderContext builderContext,
                ScriptApplicationHostOptions hostOptions)
            {
                this.builderContext = builderContext;
                this.hostOptions = hostOptions;
            }

            public void Configure(IApplicationBuilder app)
            {
                var applicationLifetime = app.ApplicationServices.GetRequiredService<IApplicationLifetime>();
                app.UseWebJobsScriptHost(applicationLifetime);
            }

            public IServiceProvider ConfigureServices(IServiceCollection services)
            {
                services.AddAuthentication()
                        .AddScriptJwtBearer()
                        .AddScheme<AuthenticationLevelOptions, AlwaysAdminAuthHandler<AuthenticationLevelOptions>>(AuthLevelAuthenticationDefaults.AuthenticationScheme, configureOptions: _ => { })
                        .AddScheme<ArmAuthenticationOptions, AlwaysAdminAuthHandler<ArmAuthenticationOptions>>(ArmAuthenticationDefaults.AuthenticationScheme, _ => { });

                services.AddWebJobsScriptHostAuthorization();
                services.AddMvc().AddApplicationPart(typeof(HostController).Assembly);

                services.AddWebJobsScriptHost(builderContext.Configuration);

                services.Configure<ScriptApplicationHostOptions>(o =>
                {
                    o.ScriptPath = hostOptions.ScriptPath;
                    o.LogPath = hostOptions.LogPath;
                    o.IsSelfHost = hostOptions.IsSelfHost;
                    o.SecretsPath = hostOptions.SecretsPath;
                });

                return services.BuildServiceProvider();
            }
        }
    }
}
