using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;

namespace InsuranceBot.Services
{
    public class CustomVisionService
    {
        private static readonly HttpClient Client = new HttpClient();

        // Add PredictionEndpoint definition here
        private static readonly CustomVisionPredictionClient _predictionClient = 
            new CustomVisionPredictionClient() { ApiKey = "<key>", Endpoint = "https://westus2.api.cognitive.microsoft.com/" };

        public async Task<string> Analyze(string imageUrl)
        {
            var image = await Client.GetByteArrayAsync(imageUrl);
            
            var result = _predictionClient.ClassifyImage(
                projectId: new Guid("<guid>"),
                publishedName: "Iteration1",
                imageData: new MemoryStream(image));
          
            return result.Predictions.OrderByDescending(x => x.Probability).First().TagName;
        }
    }
}
