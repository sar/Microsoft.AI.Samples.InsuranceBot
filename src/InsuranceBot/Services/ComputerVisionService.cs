using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace InsuranceBot.Services
{
    public class ComputerVisionService
    {
        private static readonly HttpClient Client = new HttpClient();

        // Add ComputerVisionClient definition here

        public ComputerVisionService()
        {
            // Set the endpoint here
        }

        public async Task<DetectResult> Detect(string imageUrl)
        {
            var image = await Client.GetByteArrayAsync(imageUrl);
            throw new NotImplementedException();
        }
    }

    public class DetectResult
    {
        public bool IsCar { get; set; }

        public string Description { get; set; }
    }
}
