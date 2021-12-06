using System;
using System.Runtime.Serialization;

namespace SSW.SophieBot
{
    public class SophieBotException : Exception
    {
        public SophieBotException()
        {

        }

        public SophieBotException(string message)
            : base(message)
        {

        }

        public SophieBotException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        public SophieBotException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {

        }
    }
}
