using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Script.WebHost.Authentication;
using Microsoft.Azure.WebJobs.Script.WebHost.Security.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Functions.Integration.Test.Framework.Azure.Functions
{
    public class AlwaysAdminAuthHandler<TOption>
        : AuthenticationHandler<TOption>
        where TOption : AuthenticationSchemeOptions, new()
    {
        public AlwaysAdminAuthHandler(IOptionsMonitor<TOption> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var adminLevelClaim = new Claim(SecurityConstants.AuthLevelClaimType, AuthorizationLevel.Admin.ToString());
            var claims = new List<Claim> { adminLevelClaim };
            var identity = new ClaimsIdentity(claims, AuthLevelAuthenticationDefaults.AuthenticationScheme);
            AuthenticateResult result = AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name));

            return Task.FromResult(result);
        }
    }
}
