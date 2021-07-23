using System;
using System.Text.Json;

namespace Message
{
    public class UserMsgHandler : IMessageHandler<User>
    {
        public void Process(User message)
        {
            Console.WriteLine(JsonSerializer.Serialize(message));
        }
    }
}