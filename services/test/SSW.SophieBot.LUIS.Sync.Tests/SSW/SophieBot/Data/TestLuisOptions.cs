using Microsoft.Extensions.Options;
using System;

namespace SSW.SophieBot
{
    public class TestLuisOptions : IOptions<LuisOptions>
    {
        public LuisOptions Value { get; } = new LuisOptions
        {
            AppId = Guid.NewGuid().ToString()
        };
    }
}
