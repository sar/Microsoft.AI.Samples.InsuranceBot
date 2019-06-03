using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace InsuranceBot.Services
{
    public class CustomVisionService
    {
        private static readonly HttpClient Client = new HttpClient();

        // Add PredictionEndpoint definition here

        public async Task<string> Analyze(string imageUrl)
        {
            var image = await Client.GetByteArrayAsync(imageUrl);

            throw new NotImplementedException();
        }
    }
}
