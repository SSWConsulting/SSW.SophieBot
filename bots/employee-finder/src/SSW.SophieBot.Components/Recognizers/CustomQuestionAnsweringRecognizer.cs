// Copyright (c) SSW. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Azure;
using Azure.AI.Language.QuestionAnswering;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SSW.SophieBot.Components.Recognizers
{
    /// <summary>
    /// IRecognizer implementation which uses Azure AI Language Question Answering to identify intents.
    /// This uses the official Azure.AI.Language.QuestionAnswering SDK.
    /// </summary>
    public class CustomQuestionAnsweringRecognizer : Recognizer
    {
        /// <summary>
        /// The declarative type for this recognizer.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "SSW.CustomQuestionAnsweringRecognizer";

        /// <summary>
        /// Key used when adding the intent to the <see cref="RecognizerResult"/> intents collection.
        /// </summary>
        public const string QnAMatchIntent = "QnAMatch";

        private const string IntentPrefix = "intent=";

        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings { MaxDepth = null };

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomQuestionAnsweringRecognizer"/> class.
        /// </summary>
        public CustomQuestionAnsweringRecognizer()
        {
        }

        /// <summary>
        /// Gets or sets the Project Name of your Custom Question Answering project.
        /// </summary>
        [JsonProperty("projectName")]
        public StringExpression ProjectName { get; set; }

        /// <summary>
        /// Gets or sets the endpoint URL for your Language Service.
        /// </summary>
        [JsonProperty("hostname")]
        public StringExpression HostName { get; set; }

        /// <summary>
        /// Gets or sets the API key for the Language Service.
        /// </summary>
        [JsonProperty("endpointKey")]
        public StringExpression EndpointKey { get; set; }

        /// <summary>
        /// Gets or sets the deployment name (e.g., "production" or "test").
        /// </summary>
        [JsonProperty("deploymentName")]
        public StringExpression DeploymentName { get; set; } = "production";

        /// <summary>
        /// Gets or sets the number of results you want.
        /// </summary>
        [DefaultValue(3)]
        [JsonProperty("top")]
        public IntExpression Top { get; set; } = 3;

        /// <summary>
        /// Gets or sets the threshold score to filter results.
        /// </summary>
        [DefaultValue(0.3)]
        [JsonProperty("threshold")]
        public NumberExpression Threshold { get; set; } = 0.3;

        /// <summary>
        /// Gets or sets whether to include the dialog name metadata for QnA context.
        /// </summary>
        [DefaultValue(true)]
        [JsonProperty("includeDialogNameInMetadata")]
        public BoolExpression IncludeDialogNameInMetadata { get; set; } = true;

        /// <summary>
        /// Gets or sets the flag to determine if personal information should be logged in telemetry.
        /// </summary>
        [JsonProperty("logPersonalInformation")]
        public BoolExpression LogPersonalInformation { get; set; } = "=settings.runtimeSettings.telemetry.logPersonalInformation";

        /// <summary>
        /// Gets or sets whether to enable precise answer.
        /// </summary>
        [JsonProperty("enablePreciseAnswer")]
        public BoolExpression EnablePreciseAnswer { get; set; } = true;

        /// <summary>
        /// Return results of the call to Custom Question Answering.
        /// </summary>
        public override async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
        {
            var recognizerResult = new RecognizerResult
            {
                Text = activity.Text,
                Intents = new Dictionary<string, IntentScore>(),
            };

            if (string.IsNullOrEmpty(activity.Text))
            {
                recognizerResult.Intents.Add("None", new IntentScore());
                return recognizerResult;
            }

            try
            {
                // Get configuration values
                var (epKey, error1) = EndpointKey.TryGetValue(dialogContext.State);
                var (hostname, error2) = HostName.TryGetValue(dialogContext.State);
                var (projectName, error3) = ProjectName.TryGetValue(dialogContext.State);
                var (deploymentName, error4) = DeploymentName.TryGetValue(dialogContext.State);
                var (top, _) = Top.TryGetValue(dialogContext.State);
                var (threshold, _) = Threshold.TryGetValue(dialogContext.State);
                var (enablePreciseAnswer, _) = EnablePreciseAnswer.TryGetValue(dialogContext.State);

                if (string.IsNullOrEmpty(epKey))
                {
                    throw new InvalidOperationException($"Unable to get a value for {nameof(EndpointKey)} from state. {error1}");
                }
                if (string.IsNullOrEmpty(hostname))
                {
                    throw new InvalidOperationException($"Unable to get a value for {nameof(HostName)} from state. {error2}");
                }
                if (string.IsNullOrEmpty(projectName))
                {
                    throw new InvalidOperationException($"Unable to get a value for {nameof(ProjectName)} from state. {error3}");
                }

                // Default deployment name if not specified
                if (string.IsNullOrEmpty(deploymentName))
                {
                    deploymentName = "production";
                }

                // Create the Question Answering client using Azure SDK
                var endpoint = new Uri(hostname);
                var credential = new AzureKeyCredential(epKey);
                var client = new QuestionAnsweringClient(endpoint, credential);

                // Create the project reference
                var project = new QuestionAnsweringProject(projectName, deploymentName);

                // Configure options
                var options = new AnswersOptions
                {
                    Size = top,
                    ConfidenceThreshold = threshold,
                    IncludeUnstructuredSources = true,
                    ShortAnswerOptions = enablePreciseAnswer ? new ShortAnswerOptions { ConfidenceThreshold = threshold } : null
                };

                // Add metadata filters if configured
                if (IncludeDialogNameInMetadata.GetValue(dialogContext.State))
                {
                    options.Filters = new QueryFilters();
                    options.Filters.MetadataFilter = new MetadataFilter();
                    options.Filters.MetadataFilter.LogicalOperation = LogicalOperationKind.And;
                    // Note: Metadata would be added here if needed
                }

                // Call the Question Answering service
                Response<AnswersResult> response = await client.GetAnswersAsync(activity.Text, project, options, cancellationToken).ConfigureAwait(false);

                if (response.Value.Answers != null && response.Value.Answers.Any())
                {
                    // Filter by threshold
                    var filteredAnswers = response.Value.Answers
                        .Where(a => a.Confidence >= threshold)
                        .OrderByDescending(a => a.Confidence)
                        .ToList();

                    if (filteredAnswers.Any())
                    {
                        var topAnswer = filteredAnswers.First();

                        // Check if the answer starts with "intent=" prefix
                        if (topAnswer.Answer.Trim().ToUpperInvariant().StartsWith(IntentPrefix.ToUpperInvariant(), StringComparison.Ordinal))
                        {
                            recognizerResult.Intents.Add(
                                topAnswer.Answer.Trim().Substring(IntentPrefix.Length).Trim(),
                                new IntentScore { Score = topAnswer.Confidence ?? 0 });
                        }
                        else
                        {
                            recognizerResult.Intents.Add(QnAMatchIntent, new IntentScore { Score = topAnswer.Confidence ?? 0 });
                        }

                        // Add answer to entities
                        var answerArray = new JArray();
                        
                        // Use short answer if available and enabled, otherwise use full answer
                        var answerText = enablePreciseAnswer && topAnswer.ShortAnswer != null && !string.IsNullOrEmpty(topAnswer.ShortAnswer.Text)
                            ? topAnswer.ShortAnswer.Text
                            : topAnswer.Answer;
                        
                        answerArray.Add(answerText);
                        ObjectPath.SetPathValue(recognizerResult, "entities.answer", answerArray);

                        // Add instance data
                        var instance = new JArray();
                        var data = new JObject
                        {
                            ["answer"] = topAnswer.Answer,
                            ["shortAnswer"] = topAnswer.ShortAnswer?.Text,
                            ["confidence"] = topAnswer.Confidence,
                            ["source"] = topAnswer.Source,
                            ["qnaId"] = topAnswer.QnaId,
                            ["startIndex"] = 0,
                            ["endIndex"] = activity.Text.Length
                        };
                        instance.Add(data);
                        ObjectPath.SetPathValue(recognizerResult, "entities.$instance.answer", instance);

                        // Add all answers to properties
                        var answersArray = filteredAnswers.Select(a => new
                        {
                            answer = a.Answer,
                            shortAnswer = a.ShortAnswer?.Text,
                            confidence = a.Confidence,
                            source = a.Source,
                            qnaId = a.QnaId,
                            questions = a.Questions?.ToArray()
                        }).ToArray();
                        
                        recognizerResult.Properties["answers"] = JArray.FromObject(answersArray);
                    }
                    else
                    {
                        recognizerResult.Intents.Add("None", new IntentScore { Score = 1.0f });
                    }
                }
                else
                {
                    recognizerResult.Intents.Add("None", new IntentScore { Score = 1.0f });
                }

                TrackRecognizerResult(dialogContext, "CustomQuestionAnsweringRecognizerResult", FillRecognizerResultTelemetryProperties(recognizerResult, telemetryProperties, dialogContext), telemetryMetrics);
            }
            catch (RequestFailedException ex)
            {
                // Log the error and return None intent
                System.Diagnostics.Debug.WriteLine($"Question Answering request failed: {ex.Message}");
                recognizerResult.Intents.Add("None", new IntentScore { Score = 1.0f });
                recognizerResult.Properties["error"] = ex.Message;
            }

            return recognizerResult;
        }

        /// <summary>
        /// Fill telemetry properties.
        /// </summary>
        protected override Dictionary<string, string> FillRecognizerResultTelemetryProperties(RecognizerResult recognizerResult, Dictionary<string, string> telemetryProperties, DialogContext dialogContext = null)
        {
            var properties = new Dictionary<string, string>
            {
                { "TopIntent", recognizerResult.Intents.Any() ? recognizerResult.Intents.First().Key : null },
                { "TopIntentScore", recognizerResult.Intents.Any() ? recognizerResult.Intents.First().Value?.Score?.ToString("N1", CultureInfo.InvariantCulture) : null },
                { "Intents", recognizerResult.Intents.Any() ? JsonConvert.SerializeObject(recognizerResult.Intents, _settings) : null },
                { "Entities", recognizerResult.Entities?.ToString() },
                { "AdditionalProperties", recognizerResult.Properties.Any() ? JsonConvert.SerializeObject(recognizerResult.Properties, _settings) : null },
            };

            if (dialogContext != null)
            {
                var (logPersonalInfo, _) = LogPersonalInformation.TryGetValue(dialogContext.State);
                if (logPersonalInfo && !string.IsNullOrEmpty(recognizerResult.Text))
                {
                    properties.Add("Text", recognizerResult.Text);
                    properties.Add("AlteredText", recognizerResult.AlteredText);
                }
            }

            if (telemetryProperties != null)
            {
                return telemetryProperties.Concat(properties)
                    .GroupBy(kv => kv.Key)
                    .ToDictionary(g => g.Key, g => g.First().Value);
            }

            return properties;
        }
    }
}
