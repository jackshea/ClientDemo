using System;
using System.Text.Json;
using NLog;

namespace Message
{
    public class UserMsgHandler : IMessageHandler<User>
    {
        private static ILogger log = LogManager.GetCurrentClassLogger();
        public void Process(User message)
        {
            log.Info(JsonSerializer.Serialize(message));
        }
    }
}