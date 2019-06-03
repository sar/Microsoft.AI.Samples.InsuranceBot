using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Builder.AI.Luis;

namespace InsuranceBot
{
    public class BotServices
    {
        public BotServices(BotConfiguration botConfiguration)
        {
            foreach (var service in botConfiguration.Services)
            {
                switch (service.Type)
                {
                    // Add LUIS case here
                    case ServiceTypes.Luis:
                    {
                        var luis = (LuisService)service;
                        if (luis == null)
                        {
                            throw new InvalidOperationException("The LUIS service is not configured correctly in your '.bot' file.");
                        }

                        var app = new LuisApplication(luis.AppId, luis.AuthoringKey, luis.GetEndpoint());
                        var recognizer = new LuisRecognizer(app);
                        this.LuisServices.Add(luis.Name, recognizer);
                        break;
                    }
                }
            }
        }

        // Add LUIS services property here
        public Dictionary<string, LuisRecognizer> LuisServices { get; } = new Dictionary<string, LuisRecognizer>();
    }
}
