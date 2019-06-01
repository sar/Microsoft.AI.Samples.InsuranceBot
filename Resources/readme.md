# Build 2018: Insurance Bot

In this lab, you'll get a crash course on all of the developer frameworks, APIs, and tools that Microsoft has released in the conversational AI space. You'll learn how you can build better customer experiences with intelligent bots using Microsoft Bot Framework, including how you can plug into Microsoft Cognitive Services RESTful APIs to access the latest and greatest AI capabilities.

## Getting Started

### A) Install Bot Framework Emulator

Follow the next steps to install the Bot Framework Emulator V4, if you already have it you can skip this section.

1. Open Microsoft Edge and navigate to https://github.com/Microsoft/BotFramework-Emulator/releases.
1. Download the exe file of the latest 4.x version available.
1. When prompted, click Open.
1. Complete the installation.

### B) Create Template Deployment

This lab **requires** an Azure subscription. To save time, we will be deploying required services via an ARM Template. The services to be deployed include:
  * Speech.
  * Cognitive Services: unified subscription to access Text Analytics, Computer Vision, Text Translation and Language Understanding (LUIS)

Follow the next steps to create the resources:

1. Log into the **Azure Portal** [(portal.azure.com)](https://portal.azure.com).

2. Click on **Create a resource [+]**,  search for **Template Deployment**.

3. Select this option and click the **Create** button.

4. Click **Build your own template in the editor**.

5. Click **Load file**.

6. Select `arm-template.json` from the `Downloads\InsuranceBot\Resources` folder.

7. Click **Save**.

8. Provide the required information:
    * Create a new resource group: `minilitware-group-<your initials>`.
    * Select `West US` as the location.
    * Agree to the terms and condition.
    * Click **Purchase**.

9. Once the deployment is completed you will see a **Deployment succeeded notification**.

10. Go to **Resource Groups**, in the left pane and search for the **resource group** you created in the previous steps (`minilitware-group-<your initials>`) and **open** it.

11. In the **Overview** blade, you should see **two resources**: `cs-<some-random-suffix>` and `speech-<some-random-suffix>`.

12. Open the `cs-<some-random-suffix>` resource.

13. Go to the **Keys** blade.

14. Copy the **Key 1** value to **Notepad**. That's the **Cognitive Services** key.

15. Repeat the same steps with the `speech-<some-random-suffix>` resource in your **Resource Group**, so you'll get the **Speech Services** key.

     > NOTE: We'll need both keys later on.

### C) Create Azure Web App Bot

The Azure Web App Bot is an integrated offering for building and hosting bots. It pulls together the Microsoft Bot Framework for core bot functionality and Azure Web Apps for hosting.

1. From the Azure Portal, go to the **Create a resource** blade.
1. **Search** for `Web App Bot`.
1. **Select** the first result and click the **Create** button.
1. Provide the required information:
    * Bot name: `minilitware-lab-bot-<your initials>`
    * Use your **created** resource group: `minilitware-group-<your initials>`
    * Location: `West US`
    * Pricing tier: `F0 (10K Premium Messages)`
    * App name: `minilitware-lab-bot-<your initials>`
    * Bot template: `Basic Bot C#`

      > NOTE: This bot template includes Language Understanding and it will create a LUIS Application that we will use later in the lab.
    * Azure Storage: create a new one with the recommended name
    * Application Insights Location: `West US 2`
1. Click on **App service plan/Location**.
1. Click **Create New**.
1. Provide the required information:
    * App Service plan name: `minilitware-lab-bot-<your initials>`
    * Location: `West US`
1. Click **OK** to save the new App service plan.
1. Click **Create** to deploy the service. This step might take a few moments.
> NOTE: Once the deployment is completed you will see a **Deployment succeeded** notification; however, we don't need to wait for this before proceeding to the next step of the lab. This service is *only* required if we want to run the bot in Azure. It is not required to run the bot locally with the emulator.


## Bots

### A) Run locally and debug with Bot Framework Emulator

For this lab, we'll be using Visual Studio. Let's spin up the bot locally and see the base project in action.

1. Open **Visual Studio 2017** from the Start Menu.
1. Click **Open Project/Solution**.
1. Select the solution file `Downloads\InsuranceBot\src\InsuranceBot.sln` and wait for the it to load.
1. Put **breakpoint** on line 103 of `InsuranceBot.cs` file.
1. Run the app by clicking on the **IIS Express** button in Visual Studio (with the green play icon).
1. Once the page has loaded in Microsoft Edge, **copy** the site endpoint from the address bar to the clipboard (e.g. `http://localhost:XXXX`).

    > NOTE: We won't be using the web site chat control until later in the lab. Make sure you use the emulator until then!
1. Check the port that your app is using, if it is not running on port 3978 you'll need to update the Bot configuration following the next steps:
  1. Go to Visual Studio and open the BogConfiguration.bot file.
  1. Update the endpoint setting to match the port that your app is using.
1. **Configure** the Bot Framework Emulator:
    * Open the **botframework-emulator** from the Start Menu.
    * Click Open Bot and select the file BotConfiguration.bot from your source code.
1. **Type** `hi` in the emulator.
1. Return to **Visual Studio** and wait for the breakpoint to be hit.
1. **Mouse over** the `dc` variable and **inspect** the `dc.Context.Activity` property.

     > NOTE: This is the message received from the emulator. You'll be able to see your input here (under `Text`).
1. **Remove** the breakpoint.
1. Click the **Continue** button in the toolbar.
1. Return to the **Bot Emulator** to see the response.
1. Return to **Visual Studio** and **stop** debugging by clicking the stop button in the toolbar.

### B) Add welcome message

Notice that when you start a conversation the bot is not showing any initial message, let's add a welcome message to display it to the user at the beginning of the conversations:

1. Go back to Visual Studio and open the `InsuranceBot.cs` file.
1. Around line 127 find the comment `// Add code for welcome message here`.
1. Replace the comment with the following code:
  ```cs
  if (activity.MembersAdded.Any())
  {
      // Iterate over all new members added to the conversation.
      foreach (var member in activity.MembersAdded)
      {
          if (string.Equals(member.Id, activity.Recipient.Id, StringComparison.InvariantCultureIgnoreCase))
          {
              await dc.Context.SendActivityAsync("Hi! How can I help you today?");
          }
      }
  }
  ```
1. Let's run the bot to see the welcome message:
  * Run the app by clicking on the IIS Express button in Visual Studio (with the green play icon).
  * Return to the Bot Framework Emulator.
  * Click the `Restart conversation` button to start a new conversation.
  * See the welcome message displayed at the beginning of the conversation.
  * Stop debugging by clicking the stop button in Visual Studio's toolbar.

### C) Adding the Language Understanding service to your bot

Language Understanding (LUIS) allows your application to understand what a person wants in their own words. LUIS uses machine learning to allow developers to build applications that can receive user input in natural language and extract the user's intent. LUIS has a standalone portal for managing the model and  uses Azure for subscription key management.

#### 1) Import and extend the LUIS model

In this section, we'll prepare a model that allows a user to ask for insurance.

1. Open a **new tab** in **Microsoft Edge** and go to the **LUIS portal** [(luis.ai)](https://www.luis.ai/home).
1. Click **Sign in** to log into the portal.

    > NOTE: Use the **same credentials** you used for Azure.
1. If this is your first login in this portal, you will receive a welcome message. Follow the next steps access the LUIS dashboard:
  * **Scroll down** to the bottom of the welcome page.
  * Click **Create LUIS app**.
  * Select **United States** from the country list.
  * Check the **I agree** checkbox.
  * Click the **Continue** button.
1. From My Apps, look for the following app minilitware-lab-bot-<your initials>. You will see an existing Luis App (minilitware-lab-bot-<your initials>-<some random suffix>), this was generated by the Web App Bot deployment.
1. Click on the `Manage` option in the top menu.
1. Copy the LUIS Application ID to Notepad.

    > NOTE: We'll need this app ID later on.
1. Click the `Versions` option in the left menu.
1. Click on `Import version`.
1. Select the base model from `Downloads\InsuranceBot\Resources\mini-litware.json`.
1. Click on `Done`.
1. **Wait** for the import to complete.
1. Click on the **Train** button.
1. Click the **Test** button to open the test panel.
1. **Type** the utterance `i need auto insurance` and press **enter**.

    > NOTE: It should return the `INeedInsurance` intent.
1. Click **Inspect** to see more about the intent and any entities.
1. Click the **Test** button in the top right to close the test panel.
1. Now let's add an alternate utterance for the `INeedInsurance` intent by doing:
  * Click on the `Build` option in the top menu.
  * Click on the `Intents` option in the left menu.
  * Click on the `INeedInsurance` intent in the list.
  * Review the list of utterances available for this intent.
  * In the input in the top enter `can you help me get car insurance` and press the `enter` key.
  * Check how the `car` word change for the `InsuranceType` since LUIS recognize the entity as part of the utterance.
  * Click the `Train` button to include this new utterance.
  * Click the `Test` button, enter the value `can you help me get car insurance` and press the `enter` key to see the result.
  * You can inspect the result to see how identifies the Entities.
1. Click on the `Manage` option in the top menu.
1. Click the `Keys and Endpoints` option.
1. **Copy** the `Authoring Key` to Notepad.

    > NOTE: We'll need this key later on.
1. Click on **+ Assign resource**. You might need to scroll down to find the option and do:
  * Select the **tenant**.
  * Select your **subscription**.
  * **Luis resource**: select the **Cognitive Services** resource previously created (`minilitware-cognitive-services-<your initials>`).
  * Click on **Assign resource**.
1. Publish your application:
  * Click the **Publish** button.
  * Select `Production` in the dropdown, and then click on the `Publish` button.
  * Wait for the process to finish.

#### 2) Install LUIS package

The Bot Builder SDK V4 provides a package to integrate LUIS with your bot. Follow the next steps to install the new package in the project.

Let's install the LUIS package from NuGet:

1. Right click on the `InsuranceBot` project and click Manage NuGet Packages.
1. Select the Browse tab and search for Microsoft.Bot.Builder.AI.Luis.
1. Click on the NuGet package, select the latest version and click Install.

#### 3) Add the LUIS Recognizer to your bot

Like all of the Cognitive Services, LUIS is accessible via a RESTful endpoint. However, the Bot Builder SDK has an inbuilt service component we can use to simplify this integration. This transparently calls LUIS before invoking our code, allowing our code to focus on processing the user's intent rather than natural language.

1. In Visual Studio, open `BotServices.cs`.
> NOTE: The new Bot Builder SDK uses the BotServices class to wrap up the different cognitive services clients that will be used by the bot. It uses the BotConfiguration.bot file to read the settings and initialize the services required.
1. Add the following namespace at the top of the file:
  ```cs
  using Microsoft.Bot.Builder.AI.Luis;
  ```
1. Find the comment `// Add LUIS services property here` around line 22 and replace it with the following code:
  ```cs
  public Dictionary<string, LuisRecognizer> LuisServices { get; } = new Dictionary<string, LuisRecognizer>();
  ```
1. Find the comment `// Add LUIS case here` and replace it with the following code:
  ```cs
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
  ```
1. Open the `BotConfiguration.bot` file.
1. Update the following LUIS configuration values:
  * Replace `<your_subscription_key>` with the **Key 1** value you captured in Notepad earlier for **Cognitive Services**.
  * Replace `<your_authoring_key>` and `<your_app_id>` with the values you captured in Notepad earlier from Luis portal.
1. Open the `InsuranceBot.cs`.
1. Add the following namespace at the top of the file:
  ```cs
  using Microsoft.Bot.Builder.AI.Luis;
  ```
1. Find the comment `// Replace with LUIS handler here` inside the `OnTurnAsync` method and replace all the following code:
  ```cs
  // Replace with LUIS handler here
  await dc.Context.SendActivityAsync("Hello world!");
  ```
  with the code:
  ```cs
  // Perform a call to LUIS to retrieve results for the current activity message.
  var luisResults = await _services.LuisServices.ElementAt(0).Value.RecognizeAsync(dc.Context, cancellationToken).ConfigureAwait(false);
  var topScoringIntent = luisResults?.GetTopScoringIntent();
  var topIntent = topScoringIntent.Value.intent;

  // Your code goes here
  ```
  > NOTE: The first step is to extract the LUIS intent from the context. This is populated by the middleware.

1. Replace the recently added line `// Your code goes here` with the following code:
  ```cs
  switch (topIntent)
  {
      case INeedInsuranceIntent:
          await INeedInsuranceHandler(dc, luisResults);
          break;

      default:
          await dc.Context.SendActivityAsync("Sorry, I didn't understand that.");
          break;
  }
  ```
  > NOTE: This switch will send the user's message to the right handler based on the LUIS intent name.

### D) Test the flow

LUIS is now wired into our application. Let's see it in action.

1. Run the app by clicking on the **IIS Express** button in Visual Studio (with the green play icon).
1. Return to the **botframework-emulator**.
1. Click the **Restart conversation** button to start a new conversation.
1. **Type** `i need car insurance` and press enter.

    > NOTE: Something's not quite right. We need to select the type of the insurance despite specifying **car** insurance.
1. Click on the **Car** button.

    > NOTE: We're seeing the default visualization of a set of options here.
1. Click on **Sedan** for the vehicle type.
1. **Type** `foobar` as the make.

    > NOTE: That's not a real car type. We need to be able to block bad data.
1. Return to **Visual Studio** and **stop** debugging by clicking the stop button in the toolbar.

### E) Enhance the conversation flow

Next, we'll fix a few of the issues we noticed. Namely, we'll adjust the flow if the user has specified the insurance type, improve the visualization of the insurance type options, and add some validation to the input.

1. Open **InsuranceBot.cs**.
1. **Replace** the last line of the `INeedInsuranceHandler` method with:

    ```cs
    if (string.IsNullOrEmpty(type))
    {
        await dialogContext.BeginDialogAsync(PromptStep.GatherInsuranceType);
    }
    else
    {
        await FinishInsuranceTypeStep(dialogContext, type);
    }
    ```
    > NOTE: This will adjust the conversation flow based on whether the user specified the insurance type in their message. The first line of the message shows you how we've extracted this information from LUIS.
1. In the method `AskInsuranceTypeStep` **Find**  the comment `// Add the cards definition with images` and replace it with the following code:
  ```cs
  var cards = actions
      .Select(x => new HeroCard
      {
          Images = new List<CardImage> { new CardImage(x.Image) },
          Buttons = new List<CardAction> { x },
      }.ToAttachment())
      .ToList();
  ```
  > NOTE: See how we are creating a list of HeroCards that will contain the image and the button each one.
1. In the method `AskInsuranceTypeStep` **Find** the comment `// Replace the following line to show carousel with images` in the `AskInsuranceTypeStep` method and replace the line: `var activity = (Activity)MessageFactory.Carousel(new{ heroCard.ToAttachment() }, "What kind of insurance do you need?");` with the code:
  ```cs
  var activity = (Activity)MessageFactory.Carousel(cards, "What kind of insurance do you need?");
  ```
  > NOTE: The set of hero cards will provide an image display of the insurance options. All that's needed is to pass this as part of the response activity.
1. Open the **Validators.cs** file.
1. **Replace** the contents of the `CarMakeValidator` method with the following code snippet:
    ```cs
    var value = promptContext.Recognized.Value?.Trim() ?? string.Empty;
    if (value.Equals("foobar", StringComparison.OrdinalIgnoreCase))
    {
        await promptContext.Context.SendActivityAsync("Hmmm I don't recognize that make.").ConfigureAwait(false);
        return false;
    }
    else
    {
        promptContext.Recognized.Value = value;
        return true;
    }
    ```

### F) Re-test the flow

We've fixed the bot now. Let's re-test the flow.

1. Run the app by clicking on the **IIS Express** button in Visual Studio (with the green play icon).
1. Return to the **botframework-emulator**.
1. Click the **Restart conversation** button to start a new conversation.
1. **Type** `i need insurance` and press enter.

    > NOTE: A rich visualization of the insurance types is now visible.
1. Click the **Restart conversation** button to restart the conversation.
1. **Type** `i need car insurance` and press enter.

    > NOTE: The bot processes the entity from LUIS to skip the insurance type question.
1. Click on **Sedan** for the vehicle type.
1. **Type** `foobar` as the make.

    > NOTE: The input is blocked and the user can try again.
1. Return to **Visual Studio** and **stop** debugging by clicking the stop button in the toolbar.

## Language

### A) Add Text Sentiment detection

One of the key parts of automated customer service is ensure human agents are involved at the right time. By using Text Analytics, we can detect a user's sentiment and escalate appropriately.

Follow these steps to integrate the Text Analytics API.

1. Right click on the `InsuranceBot` project and click Manage NuGet Packages.
1. Select the `Browse` tab.
1. Search for `Microsoft.Azure.CognitiveServices.Language.TextAnalytics`.
1. Click on the NuGet package, select the latest version and click Install.
1. Open the **TextSentimentService** file in the **Services** folder.
1. **Add** the following imports at the top of the class:
  ```cs
  using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
  using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
  ```
1. **Replace** the contents of the `GetTextSentiment` method with:
    ```cs
    ITextAnalyticsClient client = new TextAnalyticsClient(new ApiKeyServiceClientCredentials())
    {
        Endpoint = "https://westus.api.cognitive.microsoft.com",
    };

    var language = await client.DetectLanguageAsync(languageBatchInput: new LanguageBatchInput(documents: new List<LanguageInput>() { new LanguageInput(id: "1", text: input) }));

    var langCode = language.Documents[0].DetectedLanguages[0].Iso6391Name;
    ```
    > NOTE: When detecting sentiment, we need to specify the language. Fortunately, the Text Analytics service also provides an option to detect language (which we call first).
1. **Add** the following code snippet after the last one:
    ```cs
    var sentiment = await client.SentimentAsync(multiLanguageBatchInput: new MultiLanguageBatchInput(new List<MultiLanguageInput>() { new MultiLanguageInput(id: "1", language: langCode, text: input) }));
    
    return sentiment.Documents[0].Score.GetValueOrDefault();
    ```
1. Replace `<key>` with the **Key 1** value you captured in Notepad earlier for **Cognitive Services**.
1. Open the **InsuranceBot.cs** file.
1. **Find** the `FinalStep` method and review how the results are used.

    > NOTE: You don't need to do anything here aside from review the code.
1. Set a **breakpoint** in `FinalStep` method *after* the call to `GetTextSentiment`.

### B) Test sentiment detection

Let's run the bot to see Text Sentiment in action.

1. Run the app by clicking on the **IIS Express** button in Visual Studio (with the green play icon).
1. Return to the **botframework-emulator**.
1. Click the **Restart conversation** button to start a new conversation.
1. **Type** `I need car insurance` and press **enter**.
1. Click on **Sedan** for the vehicle type.
1. Enter **Ford** for the make, **Fusion** for the model, and **2000** for the year.
1. **Type** `no that's way too expensive` when prompted for feedback.
1. Return to **Visual Studio** and wait for the breakpoint to be hit.
1. **Remove** the breakpoint.
1. **Mouse over** the `sentiment` variable to see the result.
1. Click the **Continue** button in the toolbar.
1. Return to the **Bot Emulator** to see the response from the bot.
1. Return to **Visual Studio** and **stop** debugging by clicking the stop button in the toolbar.

### C) Add translation support

By combining the Translator Text API with our bot, we can build a solution that can interact with a global audience. The service also provides language detection, meaning users can start talking to our bot in whatever language they choose.

1. Open the **Startup.cs** file.
1. **Find** the comment `// Add translation middleware here` and replace it with the following code:
  ```cs
  var languangeProperty = conversationState.CreateProperty<string>("Language");
  options.Middleware.Add(new TranslationMiddleware(languangeProperty));
  ```
1. Open the file `TranslatorService.cs` in the folder `Services`.
1. Find the method `Detect` and replace the code inside with the following code:
  ```cs
  var uri = string.Format(TranslatorEndpoint, "detect");
  var body = new object[] { new { Text = input } };
  var requestBody = JsonConvert.SerializeObject(body);

  var message = new HttpRequestMessage(HttpMethod.Post, uri);
  message.Headers.Add("Ocp-Apim-Subscription-Key", "<your_key>");
  message.Headers.Add("Ocp-Apim-Subscription-Region", "westus");
  message.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
  message.Headers.Add("X-ClientTraceId", Guid.NewGuid().ToString());

  var response = await Client.SendAsync(message);
  response.EnsureSuccessStatusCode();
  var responseBody = await response.Content.ReadAsStringAsync();

  var result = JsonConvert.DeserializeObject<List<TranslatorDetectResponse>>(responseBody);
  var languageDetected = result.FirstOrDefault()?.Language;

  return languageDetected;
  ```
1. **Find** the method `Translate` and replace the code with the following code:
  ```cs
  var uri = $"{string.Format(TranslatorEndpoint, "translate")}&from={from}&to={to}";
  var body = new object[] { new { Text = input } };
  var requestBody = JsonConvert.SerializeObject(body);

  var message = new HttpRequestMessage(HttpMethod.Post, uri);
  message.Headers.Add("Ocp-Apim-Subscription-Key", "<your_key>");
  message.Headers.Add("Ocp-Apim-Subscription-Region", "westus");
  message.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
  message.Headers.Add("X-ClientTraceId", Guid.NewGuid().ToString());

  var response = await Client.SendAsync(message);
  response.EnsureSuccessStatusCode();
  var responseBody = await response.Content.ReadAsStringAsync();

  var result = JsonConvert.DeserializeObject<List<Dictionary<string, List<Dictionary<string, string>>>>>(responseBody);
  var translations = result[0]["translations"];
  var translation = translations[0]["text"];

  return translation;
  ```

1. Replace `<your_key>` with the **Key 1** value you captured in Notepad earlier for **Cognitive Services**.

### D) Test and improve text translation

Let's see the translation middleware in action by asking for insurance in French.

1. Run the app by clicking on the **IIS Express** button in Visual Studio (with the green play icon).
1. Return to the **botframework-emulator**.
1. Click the **Restart conversation** button to start a new conversation.
1. **Type** `J'ai besoin d'assurance` and press **enter**.

    > NOTE: The response message is translated; however, the attached cards are not.
1. Return to **Visual Studio** and **stop** debugging by clicking the stop button in the toolbar.
1. **Open** the `Startup.cs` file.
1. **Find** the comment `// Add Hero Cards translation middleware here` and replace it with the following code:
  ```cs
  options.Middleware.Add(new HeroCardsTranslationMiddleware(languangeProperty));
  ```
1. Open the `HeroCardsTranslationMiddleware.cs` inside the `Middlewares` folder and review the code to see how the attachments are translated.
1. Run the app by clicking on the **IIS Express** button in Visual Studio (with the green play icon).
1. Return to the **botframework-emulator**.
1. Click the **Restart conversation** button to start a new conversation.
1. **Type** `J'ai besoin d'assurance` and press enter.

    > NOTE: The text in the hero cards is translated now.
1. Click **Voiture** for the insurance type.
1. Click **SUV** for the vehicle type.

    > NOTE: The language no longer changes midway through the conversation.
1. Return to **Visual Studio** and **stop** debugging by clicking the stop button in the toolbar.

## Knowledge

### A) Set up QnA Maker

While the Bot Builder SDK makes building sophisticated dialog flows easy, this won't always scale well. QnA Maker can intelligently build a knowledge base of question and answer pairs and help respond to common user questions.

Setup your QnA Maker instance:

1. Return to the **Azure Portal** [(portal.azure.com)](https://portal.azure.com).
1. In the **New** blade, search for **QnA Maker**.
1. **Select** the first result and then click the **Create** button.
1. Provide the required information:
    * Name: `minilitware-qna-<your initials>`
    * Management Pricing tier: `F0 (3 Calls per second)`
    * Use the resource group that you are using for the lab.
    * Search pricing tier: select the first option.
    * Search Location: `West US`
    * App name: `minilitware-qna-<your initials>`
    * Website Location: `West US`
    * App Insights Location: `West US 2`
1. Click **Create** to deploy the service. This step might take a few moments.
1. Log into the **QnA Maker portal** [(qnamaker.ai)](https://qnamaker.ai) using your **Azure** credentials.
1. Create a knowledge base:
    * Click on **Create a knowledge base**.
    * Scroll down to **Step 2**: Connect your QnA service to your KB.
    * Select the previously created Azure service.
    * Scroll down to **Step 3**: Name your KB.
    * Enter the name of the KB: `minilitware-qna-<your initials>`.
    * Scroll down to **Step 4**: Populate your KB.
    * In the `Chit-chat` section select the `The Comic` option.
    > Note: This gives you an initial set of chit-chat data (English only), that you can edit.

    * Scroll down to **Step 5** and click on the `Create your KB` button.
1. Click the **Add QnA pair** option.
1. Add the **question** `where are you based?`.
1. Add the **answer** `Litware Insurance is headquartered in Redmond, WA and has branches across 52 different countries.`.
1. Click **Save and train**. This should take a minute.
1. Click **Publish** to start the publishing process and then **Publish** again to confirm.
1. From the sample HTTP request, get the:
    * **Host** (it should be `https://minilitware-qna-<your initials>.azurewebsites.net/qnamaker`).
    * **EndpointKey** from the Authorization header.
    * **KnowledgeBaseId** from the URI (it's a GUID).

### B) Add QnA Maker to the bot

Build, train and publish a simple question and answer bot based on FAQ URLs, structured documents, product manuals or editorial content in minutes.

1. Return to **Visual Studio**.
1. Right click on the `InsuranceBot` project and click Manage NuGet Packages.
1. Select the Browse tab and search for `Microsoft.Bot.Builder.AI.QnA`.
1. Click on the NuGet package, select the latest version and click Install.
1. Open the **Startup.cs** file.
1. **Add** the following import at the top of the class:
  ```cs
  using Microsoft.Bot.Builder.AI.QnA;
  ```
1. **Find** the `// Add QnA Maker here` comment and replace it with the following code:
  ```cs
  // Create and register a QnA service and knowledgebase
  services.AddSingleton(sp =>
  {
      return new QnAMaker(
          new QnAMakerEndpoint
          {
              EndpointKey = "<endpoint_key>",
              Host = "<host>",
              KnowledgeBaseId = "<knowledge_base_id>",
          },
          new QnAMakerOptions
          {
              ScoreThreshold = 0.9f,
              Top = 1,
          });
  });
  ```
1. Replace the `<endpoint_key>`, `<host>` and `<knowledge_base_id>` sections with the values that you copied before in previous section.
1. **Open** the ``InsuranceBot.cs`` file.
1. **Add** the following import at the top of the class:
  ```cs
  using Microsoft.Bot.Builder.AI.QnA;
  ```
1. Find the line `private readonly ILoggerFactory _loggerFactory;` and add after that the following code:
  ```cs
  private QnAMaker QnA { get; } = null;
  ```
1. Change the definition of the constructor from:
  ```cs
  public InsuranceBot(BotServices services, UserState userState, ConversationState conversationState, ILoggerFactory loggerFactory)
  ```
  to
  ```cs
  public InsuranceBot(BotServices services, UserState userState, ConversationState conversationState, QnAMaker qna, ILoggerFactory loggerFactory)
  ```
1. Find the line `_dialogStateAccessor = _conversationState.CreateProperty<DialogState>(nameof(DialogState));` and add the following line:
  ```cs
  QnA = qna ?? throw new ArgumentNullException(nameof(qna));
  ```
1. In the method `OnTurnAsync` **find** the line `await dc.Context.SendActivityAsync("Sorry, I didn't understand that.");` and replace it with the following code:
  ```cs
  var answers = await this.QnA.GetAnswersAsync(dc.Context);

  if (answers is null || answers.Count() == 0)
  {
      await dc.Context.SendActivityAsync("Sorry, I didn't understand that.");
  }
  else if (answers.Any())
  {
      // If the service produced one or more answers, send the first one.
      await dc.Context.SendActivityAsync(answers[0].Answer);
  }
  ```
1. Run the app by clicking on the **IIS Express** button in Visual Studio (with the green play icon).
1. Return to the **botframework-emulator**.
1. Click the **Restart conversation** button to start a new conversation.
1. **Type** `where are you based?` and press **enter** to see the response from the bot.
1. Return to **Visual Studio** and **stop** debugging by clicking the stop button in the toolbar.

## Vision

### A) Add Computer Vision

Now we are going to add the ability to see to our bot. The first step is to add the Computer Vision API. This general purpose computer vision API will allow us to identify whether the picture contains a vehicle (or something else).

1. Open **InsuranceBot.cs** and find the constructor.
1. In the constructor, **find** the commented step `// PromptForCarPictureStepAsync` inside the `gatherInfoWaterfallSteps` var.
1. Uncommented the line to include this step.
1. In the constructor again **find** the comment `// Add picture prompt here` and uncomment the next 5 lines to make it look like:
  ```cs
  Dialogs.Add(new AttachmentPrompt(PromptStep.CarPicturePrompt, async (promptValidatorContext, cancellationToken) =>
  {
      var ineedInsuranceState = await _ineedInsuranceStateAccessor.GetAsync(promptValidatorContext.Context);
      return await Validator.CarPictureValidator(promptValidatorContext, ineedInsuranceState.CarType);
  }));
  ```
1. Open the **Validator.cs** file.
1. **Find** the `CarPictureValidator` method and replace the code with the following code:
  ```cs
  var computerVisionService = new ComputerVisionService();
  var detectResult = await computerVisionService.Detect(promptContext.Recognized.Value[0].ContentUrl);
  if (!detectResult.IsCar)
  {
      await promptContext.Context.SendActivityAsync($"That doesn't look like a car. It looks more like {detectResult.Description}.");
      return false;
  }

  // Add Custom Vision validation here

  return true;
  ```
1. Set a **breakpoint** on the first line of the `CarPictureValidator` method.
1. Open **ComputerVisionService.cs** in the **Services** folder.
1. Right click on the `InsuranceBot` project and click Manage NuGet Packages.
1. Select the Browse tab and search for `Microsoft.Azure.CognitiveServices.Vision.ComputerVision`.
1. Click on the NuGet package, select the latest version and click Install.
1. **Add** the following import at the top of the class:
  ```cs
  using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
  using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
  ```
1. **Find** the comment `// Add ComputerVisionClient definition here` and replace it with the following code:
  ```cs
  private readonly ComputerVisionClient _computerVisionApi = new ComputerVisionClient(
            new ApiKeyServiceClientCredentials("<api_key>"),
            new System.Net.Http.DelegatingHandler[] { });
  ```
1. **Find** the comment `// Set the endpoint here` and replace it with the following code:
  ```cs
  _computerVisionApi.Endpoint = "https://westus.api.cognitive.microsoft.com";
  ```
1. **Find** the method `Detect` and replace the line `throw new NotImplementedException();` with the following code:
  ```cs
  var results = await _computerVisionApi.AnalyzeImageInStreamAsync(
      new MemoryStream(image),
      new List<VisualFeatureTypes> { VisualFeatureTypes.Tags, VisualFeatureTypes.Description, VisualFeatureTypes.Categories });

  return new DetectResult
  {
      IsCar = results.Tags.Any(x => x.Name == "car") || results.Categories.Any(x => x.Name.Contains("trans_car")),
      Description = results.Description.Captions.First().Text,
  };
  ```
  > NOTE: Notice that we need to explicitly specify the visual features we want the API to examine.
1. Replace `<api_key>` with the **Key 1** value you captured in Notepad earlier for **Cognitive Services**.

### B) Test computer vision capabilities

1. Run the app by clicking on the **IIS Express** button in Visual Studio (with the green play icon).
1. Return to the **botframework-emulator**.
1. Click the **Restart conversation** button to start a new conversation.
1. **Type** `I need car insurance` and press **enter**.
1. Click on **Sedan** for the vehicle type.
1. Enter **Ford** for the make, **Fusion** for the model, and **2000** for the year.
1. Click the **upload** button and provide `Downloads\InsuranceBot\Resources\dog.jpg`.
1. Return to **Visual Studio** and wait for the breakpoint to be hit.
1. **Step over** the call to `computerVisionService.Detect`.
1. **Mouse over** the `detectResult` variable to see the response.

    > NOTE: The image is correctly identified as *not* a car. It also provides a text description of image!
1. Click the **Continue** button in the toolbar.
1. Return to the **Bot Emulator** to see the response from the bot.
1. **Stop** debugging by clicking the stop button in the toolbar.

### C) Add Custom Vision

While Computer Vision can identify whether the image contains a vehicle, it can't detect the type (e.g. sports car or sedan). For this purpose, we'll train a Custom Vision model.

#### 1) Create and train model

In this section we are going to create a new model that will be able to identify the type of a car to be use in our project.

1. From the Azure Portal, go to the **Create a resource** blade.
1. **Search** for `Custom Vision`.
1. **Select** the first result and click the **Create** button.
1. Provide the required information:
   * Bot name: `minilitware-lab-<your initials>`
   * Subscription: **Pay-As-You-Go**.
   * Location: `West US 2`.
   * Prediction pricing tier: `F0`.
   * Training pricing tier: `F0`.
   * Resource group: the one you created for this lab.
1. Click **Create**.
1. Now that you've created your **Custom Vision** resource, go to Edge and navigate to **https://customvision.ai** [(customvision.ai)](https://customvision.ai).
1. Login with your azure account.
1. Click on the `NEW PROJECT` option in the main container.
1. Enter the required information:
  * Name: `minilitware-lab-<your initials>`
  * Resource group: select the resource group that you are using in this lab.
  * Project Types: select the `Classification` option.
  * Classification Types: select `Multiclass (Single tag per image)`.
  * Domains: select `General`.
1. Wait for the project to be created.
1. Click on the `Add images` button.
1. Navigate to the folder `Downloads\InsuranceBot\Resources\Custom Vision Model Images\Sedan` and select the 5 images and click `Open`.
1. In the input for `My Tags` enter the tag value `sedan`.
1. Click the `Upload 5 files` to upload all the images.
1. Wait for the process to be done and click the `Done` button.
1. Click on the `Add images` button.
1. Navigate to the folder `Downloads\InsuranceBot\Resources\Custom Vision Model Images\Sport` and select the 5 images and click `Open`.
1. In the input for `My Tags` enter the tag value `sport`.
1. Click the `Upload 5 files` to upload all the images.
1. Wait for the process to be done and click the `Done` button.
1. Click on the `Add images` button.
1. Navigate to the folder `Downloads\InsuranceBot\Resources\Custom Vision Model Images\Suv` and select the 5 images and click `Open`.
1. In the input for `My Tags` enter the tag value `suv`.
1. Click the `Upload 5 files` to upload all the images.
1. Wait for the process to be done and click the `Done` button.
1. Click on the `Train` button in the top.
1. After training is done and the iteration information is show, click on the `Publish` button.
1. Set a camel case name as the `Publish name`. For example, you can use `minilitwareLab` as publish name.
1. Once the publish is done, click on the settings icon in the top right corner to see the information of the project.
1. Copy the `Project Id` and `Prediction-Key` values that we will be use later.

#### 2) Adding custom vision to the project

Let's start adding the required package to the project.

1. Right click on the `InsuranceBot` project and click Manage NuGet Packages.
1. Select the Browse tab and search for `Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction`.
1. Click on the NuGet package, select the latest version and click Install.
1. Open **CustomVisionService.cs** in the **Services** folder.
1. **Add** the following import at the top of the class:
  ```cs
  using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
  ```
1. **Find** the comment `// Add PredictionEndpoint definition here` and replace it with the following code:
  ```cs
  private static readonly CustomVisionPredictionClient _predictionClient = 
            new CustomVisionPredictionClient() { ApiKey = "<prediction_key>", Endpoint = "https://westus2.api.cognitive.microsoft.com/" };
  ```
> NOTE: The endpoint is already there, but be sure to change it if you create the project in a different region than West US 2.

1. **Update** the `Analyze` method to replace the `throw new NotImplementedException();` in the with the following code:
  ```cs
  var result = _predictionClient.ClassifyImage(
                projectId: new Guid("<your_project_id>"),
                publishedName: "<published iteration name>",
                imageData: new MemoryStream(image));
            
            return result.Predictions.OrderByDescending(x => x.Probability).First().TagName;
  ```
1. **Replace** `<prediction_key> ` with the `Prediction-Key` value that you copied.
1. **Replace** `<your_project_id>` with the `Project Id` value that you copied.
1. **Replace** `<published_iteration_name>` with the `Published iteration name` value that you set to the iteration (`minilitwareLab` ).
1. Open the **Validator.cs** file.
1. **Find** the comment `// Add Custom Vision validation here` and replace it with the following code:
  ```cs
  var customVisionService = new CustomVisionService();
  var predictedCarType = await customVisionService.Analyze(promptContext.Recognized.Value[0].ContentUrl);
  var isRightCarType = string.Equals(predictedCarType, carType, StringComparison.OrdinalIgnoreCase);
  if (!isRightCarType)
  {
      await promptContext.Context.SendActivityAsync($"That doesn’t look like a {carType}.");
      return false;
  }
  ```

### D) Test custom vision capabilities

Now that we're feature complete, it's time to run through the bot from end-to-end.

1. Run the app by clicking on the **IIS Express** button in Visual Studio (with the green play icon).
1. Return to the **botframework-emulator**.
1. Click the **Restart conversation** button to start a new conversation.
1. **Type** `I need car insurance` and press **enter**.
1. Click on **Sedan** for the vehicle type.
1. Enter **Ford** for the make, **Fusion** for the model, and **2000** for the year.
1. Click the **upload** button and provide `Downloads\InsuranceBot\Resources\dog.jpg`.
1. Return to **Visual Studio** and wait for the breakpoint to be hit.
1. **Step over** the call to `computerVisionService.Detect`.
1. **Mouse over** the `detectResult` variable to see the response.

    > NOTE: The image is correctly identified as *not* a car. It also provides a text description of image!
1. Click the **Continue** button in the toolbar.
1. Click the **upload** button and provide `Downloads\InsuranceBot\Resources\dream_car.jpg`.
1. Return to **Visual Studio** and wait for the breakpoint to be hit.
1. **Step over** the call to `customVisionService.Analyze`.
1. **Mouse over** the `predictedCarType` variable to see the response.

     > NOTE: The image is correctly identified as a car by Computer Vision. The Custom Vision service blocks it because it's not a sedan (like we told the bot earlier).
1. Click the **Continue** button in the toolbar.
1. Click the **upload** button and provide `Downloads\InsuranceBot\Resources\ford_fusion.jpg`.
1. Return to **Visual Studio** and wait for the breakpoint to be hit.
1. **Step over** the call to `customVisionService.Analyze`.

     > NOTE: Success! The image uploaded matches the type of car we specified earlier.
1. **Remove** the breakpoint.
1. Click the **Continue** button in the toolbar.
1. Return to the **Bot Emulator** to see the response from the bot.
1. **Stop** debugging by clicking the stop button in the toolbar.

## Working with the Web Chat

### A) Setup Web Chat

We will connect the Web Chat in our html with our bot using the DirectLine channel, following we are going setup this channel in Azure.

1. Return to the **Azure Portal** [(portal.azure.com)](https://portal.azure.com).
1. Go to **All Resources** in the left pane and **search** for your bot (`minilitware-lab-bot-<your initials>`).
1. Click on the **Web App Bot** to open it.
1. Click on the **Channels**.
1. Under **Add a featured channel**, select **Configure Direct Line Channel**.
1. In the **Secret Keys** section, click the **Show** toggle button to display the password.
1. **Copy** the password to clipboard.
1. **Return** to Visual Studio.
1. Open **default.html** in the **wwwroot** folder.
1. Between `<head>` and `</head>` in line 4 add the following code to include the web chat files and a few styles to our page:
  ```html
  <link href="https://cdn.botframework.com/botframework-webchat/master/botchat.css" rel="stylesheet" />
  <script src="https://cdn.botframework.com/botframework-webchat/master/webchat.js"></script>
  <style>
    html, body {
      height: 100%
    }

    body {
      margin: 0
    }

    #webchat,
    #webchat > * {
      height: 100%;
      width: 100%;
    }
  </style>
  ```
1. In the body find the line `<h1>MiniLitware Lab</h1>` and replace it with the following code:
  ```html
  <div id="webchat"></div>
  <script>
    (async function () {
      const authorizationToken = await fetch('https://westus.api.cognitive.microsoft.com/sts/v1.0/issuetoken', {
        method: 'post',
        headers: {
          "Ocp-Apim-Subscription-Key": "speech-subscription-key",
        }
      }).then(res => res.text());

      let webSpeechPonyfillFactory = await window.WebChat.createCognitiveServicesSpeechServicesPonyfillFactory({ authorizationToken: authorizationToken, region: 'westus' });

      window.WebChat.renderWebChat({
        directLine: window.WebChat.createDirectLine({ secret: 'direct-line-secret' }),
        webSpeechPonyfillFactory
      }, document.getElementById('webchat'));

      document.querySelector('#webchat > *').focus();
    })().catch(err => console.error(err));
  </script>
  ```
  > NOTE: First line define a container where the web chat will render. Then we are adding the required code to initialize the web chat.
1. **Replace** the `direct-line-secret` value with the value you got from Azure portal.

### B) Setup Speech

Speech is available by as a feature in the Web Chat control provided by the Bot Framework team. Before we can set up the speech capabilities, we need to configure the web chat control.

1. Return to **Visual Studio** and open the file `default.html` in the `wwwroot` folder.
1. Replace `speech-subscription-key` with the **Key 1** value you captured in Notepad earlier for **Speech Services**.
1. Speech will be tested after deploy in the following section since we need a secure environment to be able to use it.

## Publish to Azure

Now that our development efforts are complete, it's time to publish the bot to Azure.

1. Sign in in  **Visual Studio** with the same credentials as you used for **Azure**.
  > NOTE: This will connect Visual Studio to your Azure subscription.
1. **Right-click** the `InsuranceBot` project.
1. Click **Publish**.
1. **Check** the option `Select Existing`.
1. Click **Publish**.
1. Select the Resource group that you are using and the bot resource.
1. Click **OK**.
1. **Wait** for the deployment to complete. This operation might take a few minutes.

    > NOTE: Once finished, the site will open in Microsoft Edge.
1. **Wait** for the page to load and to receive the hi message in the chat from the Bot.
1. **Type** in `I need insurance` and press **enter**.

    > NOTE: The bot will respond just like in the emulator.
1. **Reload** the page to try the Speech functionality.
1. **Wait** for the page to load, you should see the welcome message from the Bot.
1. **Click** on the microphone icon and say `I need insurance`.
1. The speech service should receive the audio and return the text to the web chat.
1. The Bot will received the message and respond also with audio and will expect another message as an audio automatically.

     > NOTE: If the speech functionality is not working, make sure that your browser is configured to use the `English (United States)` language.

## Conclusion

Today you've seen how Cognitive Services can combine with the Bot Framework and SDK to build powerful intelligent customer service experiences. In just over an hour, we've assembled a bot that can understand natural language, speech, vision, and knowledge.
