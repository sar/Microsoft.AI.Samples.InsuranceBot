using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using InsuranceBot.Models;
using Newtonsoft.Json;

namespace InsuranceBot.Services
{
    public class TranslatorService
    {
        private const string TranslatorEndpoint = "https://api.cognitive.microsofttranslator.com/{0}?api-version=3.0";
        private static readonly HttpClient Client = new HttpClient();

        public static async Task<string> Detect(string input)
        {
            throw new NotImplementedException();
        }

        public static async Task<string> Translate(string input, string from, string to)
        {
            throw new NotImplementedException();
        }
    }
}
