﻿using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using SSWSophieBot.Components;
using SSWSophieBot.HttpClientAction.Models;
using System;

namespace SSWSophieBot.HttpClientComponents.PersonQuery.Actions
{
    public class SkillExpression
    {
        [JsonProperty("technology")]
        public StringExpression Technology { get; set; }

        [JsonProperty("experienceLevel")]
        public StringExpression ExperienceLevelString { get; set; }

        public ExperienceLevel? GetExperienceLevel(DialogContext dc)
        {
            var experienceLevel = dc?.GetValue(ExperienceLevelString);
            if (!string.IsNullOrWhiteSpace(experienceLevel))
            {
                if (experienceLevel.Equals(ExperienceLevel.Advanced.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return ExperienceLevel.Advanced;
                }
                else if (experienceLevel.Equals(ExperienceLevel.Intermediate.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return ExperienceLevel.Intermediate;
                }
            }

            return null;
        }
    }
}