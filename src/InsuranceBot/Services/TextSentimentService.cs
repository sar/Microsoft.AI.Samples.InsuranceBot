using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;

namespace InsuranceBot.Services
{
    public class TextSentimentService
    {
        public static async Task<double> GetTextSentiment(string input)
        {
            return 0.5;
        }

        /// <summary>
        /// Container for subscription credentials. Make sure to enter your valid key.
        /// </summary>
        private class ApiKeyServiceClientCredentials : ServiceClientCredentials
        {
            public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                request.Headers.Add("Ocp-Apim-Subscription-Key", "<key>");
                return base.ProcessHttpRequestAsync(request, cancellationToken);
            }
        }
    }
}
