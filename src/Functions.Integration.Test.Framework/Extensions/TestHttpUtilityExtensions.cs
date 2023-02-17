using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace System.Net.Http
{
    public static class TestHttpUtilityExtensions
    {
        public static async Task<HttpResponseMessage?> QueryResponseUntilDone(this HttpClient client, Func<HttpClient, Task<HttpResponseMessage>> execute)
        {
            var response = await execute.Invoke(client);

            if (response.StatusCode != HttpStatusCode.Accepted)
            {
                return response;
            }

            string responseUri = response.Headers.GetValues("location").FirstOrDefault();

            if (responseUri == null)
            {
                throw new Exception("Missing location header from response");
            }

            while (response.StatusCode == HttpStatusCode.Accepted)
            {
                response = await client.GetAsync(responseUri);
            }

            return response;
        }
    }
}
