using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Functions.Integration.Test.Framework.Extensions
{
    public static class HttpClientExtensions
    {
        public static async Task<bool> IsHostRunningAsync(this HttpClient client)
        {
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, string.Empty))
            {
                try
                {
                    using (HttpResponseMessage response = await client.SendAsync(request))
                    {
                        return response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.OK;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
