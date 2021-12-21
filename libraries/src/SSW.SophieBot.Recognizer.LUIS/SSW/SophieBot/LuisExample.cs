using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using System;
using System.Collections.Generic;

namespace SSW.SophieBot
{
    public class LuisExample : BasicExample
    {
        public LuisExample(FormattableString formatText) : base(formatText)
        {

        }

        public virtual List<EntityLabelObject> GetEntityLabelObjects()
        {
            var entityLabelObjects = new List<EntityLabelObject>();
            var rawText = ToString();

            if (rawText.IsNullOrEmpty())
            {
                return entityLabelObjects;
            }

            var charIndexes = GetLabelTextIndexesRecursively();

            foreach (var exampleLabel in GetAllExampleLabels())
            {
                var labelObject = new EntityLabelObject(
                    ModelAttribute.GetName(exampleLabel.EntityType),
                    charIndexes[exampleLabel].Item1,
                    charIndexes[exampleLabel].Item2);
                entityLabelObjects.Add(labelObject);
            }

            return entityLabelObjects;
        }

        private Dictionary<ExampleLabel, (int, int)> GetLabelTextIndexesRecursively(
            ExampleLabel label = null,
            Dictionary<ExampleLabel, (int, int)> indexDic = null)
        {
            indexDic ??= new Dictionary<ExampleLabel, (int, int)>();
            CalculateCharIndex((BasicExample)label ?? this, indexDic);

            var exampleLabels = label?.ChildLabels ?? ChildLabels;

            foreach (var childLabel in exampleLabels)
            {
                GetLabelTextIndexesRecursively(childLabel, indexDic);
            }

            return indexDic;
        }

        private void CalculateCharIndex(BasicExample parent, Dictionary<ExampleLabel, (int, int)> indexDic)
        {
            var parentText = parent.ToString();
            var totalSkipCharCount = 0;

            foreach (var childLabel in parent.ChildLabels)
            {
                var childText = childLabel.ToString();
                var childLabelIndex = parentText.IndexOf(childText);

                var startCharIndex = CalculateStartCharIndexRecursively(parent, childLabelIndex + totalSkipCharCount, indexDic);
                indexDic[childLabel] = (startCharIndex, startCharIndex + childText.Length - 1);

                var skipCharCount = childLabelIndex + childText.Length;

                if (skipCharCount >= parentText.Length)
                {
                    return;
                }

                parentText = parentText.Substring(skipCharCount);
                totalSkipCharCount += skipCharCount;
            }
        }

        private int CalculateStartCharIndexRecursively(
            BasicExample parent,
            int childStartCharIndex,
            Dictionary<ExampleLabel, (int, int)> indexDic)
        {
            var startCharIndex = childStartCharIndex;

            if (parent is ExampleLabel label)
            {
                startCharIndex += CalculateStartCharIndexRecursively(label.Parent, indexDic[label].Item1, indexDic);
            }

            return startCharIndex;
        }

        public static implicit operator LuisExample(FormattableString formatText)
        {
            return new LuisExample(formatText);
        }
    }
}
