using Microsoft.Extensions.Options;
using System;

namespace SSW.SophieBot.LUIS.Sync.Test.Data
{
    public class TestLuisOptions : IOptions<LuisOptions>
    {
        public LuisOptions Value { get; } = new LuisOptions
        {
            AppId = Guid.NewGuid().ToString()
        };
    }
}
