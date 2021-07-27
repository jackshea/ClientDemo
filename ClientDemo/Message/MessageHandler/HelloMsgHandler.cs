using System;
using System.Text.Json;
using NLog;

namespace Message
{
    public class HelloMsgHandler : IMessageHandler<Hello>
    {
        private static ILogger log = LogManager.GetCurrentClassLogger();
        public void Process(Hello message)
        {
            log.Info(JsonSerializer.Serialize(message));
        }
    }
}