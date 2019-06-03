using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InsuranceBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace InsuranceBot
{
    public class InsuranceBot : IBot
    {
        private const string Site = "https://insurance.litwaredemos.com/images";

        // Supported LUIS Intents
        public const string INeedInsuranceIntent = "INeedInsurance";

        private readonly IStatePropertyAccessor<BotState> _ineedInsuranceStateAccessor;
        private readonly IStatePropertyAccessor<DialogState> _dialogStateAccessor;
        private readonly UserState _userState;
        private readonly ConversationState _conversationState;
        private readonly BotServices _services;
        private readonly ILoggerFactory _loggerFactory;

        public InsuranceBot(BotServices services, UserState userState, ConversationState conversationState, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _userState = userState ?? throw new ArgumentNullException(nameof(userState));
            _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            _ineedInsuranceStateAccessor = _userState.CreateProperty<BotState>(nameof(BotState));
            _dialogStateAccessor = _conversationState.CreateProperty<DialogState>(nameof(DialogState));

            Dialogs = new DialogSet(_dialogStateAccessor);

            // Add control flow dialogs
            var waterfallSteps = new WaterfallStep[]
            {
                InitializeStateStepAsync,
                AskInsuranceTypeStep,
                FinishInsuranceTypeStep,
            };
            Dialogs.Add(new WaterfallDialog(PromptStep.GatherInsuranceType, waterfallSteps));
            Dialogs.Add(new TextPrompt(PromptStep.InsuranceTypePrompt));

            // Add control flow dialogs
            var gatherInfoWaterfallSteps = new WaterfallStep[]
            {
                InitializeStateStepAsync,
                PromptForCarTypeStepAsync,
                PromptForCarMakeStepAsync,
                PromptForCarModelStepAsync,
                PromptForCarYearStepAsync,
                // PromptForCarPictureStepAsync,
                PromptForUserFeedbackStepAsync,
                FinalStep,
            };

            Dialogs.Add(new WaterfallDialog(PromptStep.GatherInfo, gatherInfoWaterfallSteps));
            Dialogs.Add(new TextPrompt(PromptStep.CarTypePrompt));
            Dialogs.Add(new TextPrompt(PromptStep.CarMakePrompt, Validator.CarMakeValidator));
            Dialogs.Add(new TextPrompt(PromptStep.CarModelPrompt, Validator.CarModelValidator));
            Dialogs.Add(new NumberPrompt<int>(PromptStep.CarYearPrompt, Validator.CarYearValidator));
            // Add picture prompt here
            //Dialogs.Add(new AttachmentPrompt(PromptStep.CarPicturePrompt, async (promptValidatorContext, cancellationToken) =>
            //{
            //    var ineedInsuranceState = await _ineedInsuranceStateAccessor.GetAsync(promptValidatorContext.Context);
            //    return await Validator.CarPictureValidator(promptValidatorContext, ineedInsuranceState.CarType);
            //}));
            Dialogs.Add(new TextPrompt(PromptStep.UserFeedbackPrompt, Validator.UserFeedbackValidator));
        }

        private DialogSet Dialogs { get; set; }

        /// <summary>
        /// Run every turn of the conversation. Handles orchestration of messages.
        /// </summary>
        /// <param name="turnContext">Bot Turn Context.</param>
        /// <param name="cancellationToken">Task CancellationToken.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;

            // Create a dialog context
            var dc = await Dialogs.CreateContextAsync(turnContext);

            if (activity.Type == ActivityTypes.Message)
            {
                // Continue the current dialog
                var dialogResult = await dc.ContinueDialogAsync();

                // if no one has responded,
                if (!dc.Context.Responded)
                {
                    // examine results from active dialog
                    switch (dialogResult.Status)
                    {
                        case DialogTurnStatus.Empty:
                            // Replace with LUIS handler here
                            await dc.Context.SendActivityAsync("Hello world!");

                            break;

                        case DialogTurnStatus.Waiting:
                            // The active dialog is waiting for a response from the user, so do nothing.
                            break;

                        case DialogTurnStatus.Complete:
                            await dc.EndDialogAsync();
                            break;

                        default:
                            await dc.CancelAllDialogsAsync();
                            break;
                    }
                }
            }
            else if (activity.Type == ActivityTypes.ConversationUpdate)
            {
                // Add code for welcome message here
            }

            await _conversationState.SaveChangesAsync(turnContext);
            await _userState.SaveChangesAsync(turnContext);
        }

        private async Task INeedInsuranceHandler(DialogContext dialogContext, RecognizerResult result)
        {
            var type = (string)result.Entities["InsuranceType"]?[0];
            await dialogContext.BeginDialogAsync(PromptStep.GatherInsuranceType);
        }

        private async Task<DialogTurnResult> InitializeStateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var ineedInsuranceState = await _ineedInsuranceStateAccessor.GetAsync(stepContext.Context, () => null);
            if (ineedInsuranceState == null)
            {
                var ineedInsuranceStateOpt = stepContext.Options as BotState;
                if (ineedInsuranceStateOpt != null)
                {
                    await _ineedInsuranceStateAccessor.SetAsync(stepContext.Context, ineedInsuranceStateOpt);
                }
                else
                {
                    await _ineedInsuranceStateAccessor.SetAsync(stepContext.Context, new BotState());
                }
            }

            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> AskInsuranceTypeStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var ineedInsuranceState = await _ineedInsuranceStateAccessor.GetAsync(stepContext.Context);

            var actions = new[]
            {
                new CardAction(type: ActionTypes.ImBack, title: "Car", value: "Car", image: $"{Site}/auto_600x400.png"),
                new CardAction(type: ActionTypes.ImBack, title: "Property", value: "Property", image: $"{Site}/property_600x400.jpg"),
                new CardAction(type: ActionTypes.ImBack, title: "Life", value: "Life", image: $"{Site}/life_600x400.jpg"),
            };
            var heroCard = new HeroCard(buttons: actions);

            // Add the cards definition with images

            // Replace the following line to show carousel with images
            var activity = (Activity)MessageFactory.Carousel(new[] { heroCard.ToAttachment() }, "What kind of insurance do you need?");
            return await stepContext.PromptAsync(PromptStep.InsuranceTypePrompt, new PromptOptions { Prompt = activity }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinishInsuranceTypeStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await FinishInsuranceTypeStep(stepContext, stepContext.Result as string);
        }

        private async Task<DialogTurnResult> FinishInsuranceTypeStep(DialogContext stepContext, string insuranceType)
        {
            var context = stepContext.Context;
            if (string.Equals(insuranceType, "car", StringComparison.OrdinalIgnoreCase))
            {
                return await stepContext.ReplaceDialogAsync(PromptStep.GatherInfo);
            }
            else
            {
                await context.SendActivityAsync($"Right now I can only help with car insurance: {insuranceType}");
                return await stepContext.EndDialogAsync();
            }
        }

        private async Task<DialogTurnResult> PromptForCarTypeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var actions = new[]
            {
                new CardAction(type: ActionTypes.ImBack, title: "Sedan", value: "Sedan"),
                new CardAction(type: ActionTypes.ImBack, title: "SUV", value: "SUV"),
                new CardAction(type: ActionTypes.ImBack, title: "Sports car", value: "Sports car"),
            };

            var choices = actions.Select(x => new Choice { Action = x, Value = (string)x.Value }).ToList();
            var heroCard = new HeroCard(buttons: actions);
            var activity = (Activity)MessageFactory.Carousel(new[] { heroCard.ToAttachment() }, "Please select a car type.");
            return await stepContext.PromptAsync(PromptStep.CarTypePrompt, new PromptOptions { Prompt = activity }, cancellationToken);
        }

        private async Task<DialogTurnResult> PromptForCarMakeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var ineedInsuranceState = await _ineedInsuranceStateAccessor.GetAsync(stepContext.Context);
            ineedInsuranceState.CarType = stepContext.Result as string;
            await _ineedInsuranceStateAccessor.SetAsync(stepContext.Context, ineedInsuranceState);

            var opts = new PromptOptions
            {
                Prompt = new Activity
                {
                    Type = ActivityTypes.Message,
                    Text = "What make of car do you want to insure?",
                },
            };
            return await stepContext.PromptAsync(PromptStep.CarMakePrompt, opts);
        }

        private async Task<DialogTurnResult> PromptForCarModelStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var ineedInsuranceState = await _ineedInsuranceStateAccessor.GetAsync(stepContext.Context);
            ineedInsuranceState.CarMake = stepContext.Result as string;
            await _ineedInsuranceStateAccessor.SetAsync(stepContext.Context, ineedInsuranceState);

            var opts = new PromptOptions
            {
                Prompt = new Activity
                {
                    Type = ActivityTypes.Message,
                    Text = "And the model?",
                },
            };
            return await stepContext.PromptAsync(PromptStep.CarModelPrompt, opts);
        }

        private async Task<DialogTurnResult> PromptForCarYearStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var ineedInsuranceState = await _ineedInsuranceStateAccessor.GetAsync(stepContext.Context);
            ineedInsuranceState.CarModel = stepContext.Result as string;
            await _ineedInsuranceStateAccessor.SetAsync(stepContext.Context, ineedInsuranceState);

            var opts = new PromptOptions
            {
                Prompt = new Activity
                {
                    Type = ActivityTypes.Message,
                    Text = "And the year?",
                },
            };
            return await stepContext.PromptAsync(PromptStep.CarYearPrompt, opts);
        }

        private async Task<DialogTurnResult> PromptForCarPictureStepAsync(WaterfallStepContext stepContext, CancellationToken cancellation)
        {
            var ineedInsuranceState = await _ineedInsuranceStateAccessor.GetAsync(stepContext.Context);
            ineedInsuranceState.CarYear = (int)stepContext.Result;
            await _ineedInsuranceStateAccessor.SetAsync(stepContext.Context, ineedInsuranceState);

            var opts = new PromptOptions
            {
                Prompt = new Activity
                {
                    Type = ActivityTypes.Message,
                    Text = "Please upload an image of your car to continue.",
                },
            };
            return await stepContext.PromptAsync(PromptStep.CarPicturePrompt, opts);
        }

        private async Task<DialogTurnResult> PromptForUserFeedbackStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("We can insure your new car for just $116.25 per month.This includes coverage for your whole family and a 10 % discount given your existing policy with us.");
            await Task.Delay(2000);

            var opts = new PromptOptions
            {
                Prompt = new Activity
                {
                    Type = ActivityTypes.Message,
                    Text = "What do you think? If this sounds good, we can start your coverage right now!",
                },
            };
            return await stepContext.PromptAsync(PromptStep.UserFeedbackPrompt, opts);
        }

        private async Task<DialogTurnResult> FinalStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var ineedInsuranceState = await _ineedInsuranceStateAccessor.GetAsync(stepContext.Context);
            var feedback = stepContext.Result as string;
            var sentiment = await TextSentimentService.GetTextSentiment(feedback);
            if (sentiment < 0.5)
            {
                await stepContext.Context.SendActivityAsync("I understand. We really want to make it work. Let me see if a customer service agent is available to review this in more detail.");
            }
            else
            {
                await stepContext.Context.SendActivityAsync("Great! We are going to prepare everything for you.");
            }

            return await stepContext.EndDialogAsync();
        }
    }

    public static class PromptStep
    {
        public const string GatherInsuranceType = "gatherInsuranceType";
        public const string InsuranceTypePrompt = "insuranceTypePrompt";

        public const string GatherInfo = "gatherInfo";
        public const string CarTypePrompt = "carTypePrompt";
        public const string CarMakePrompt = "carMakePrompt";
        public const string CarModelPrompt = "carModelPrompt";
        public const string CarYearPrompt = "carYearPrompt";
        public const string CarPicturePrompt = "carPicturePrompt";
        public const string UserFeedbackPrompt = "userFeedbackPrompt";
    }
}
