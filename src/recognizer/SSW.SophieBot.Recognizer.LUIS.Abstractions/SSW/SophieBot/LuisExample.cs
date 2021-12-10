using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SSW.SophieBot
{
    public class LuisExample : BasicExample
    {
        public LuisExample(FormattableString formatText) : base(formatText)
        {

        }

        public virtual List<EntityLabelObject> ToEntityLabelObjects()
        {
            var entityLabelObjects = new List<EntityLabelObject>();
            var rawText = RawText();

            if(rawText.IsNullOrEmpty())
            {
                return entityLabelObjects;
            }

            foreach(var token in this)
            {
                
            }
        }
    }
}
