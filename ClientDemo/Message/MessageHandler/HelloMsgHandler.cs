using System;
using System.Text.Json;

namespace Message
{
    public class HelloMsgHandler : IMessageHandler<Hello>
    {
        public void Process(Hello message)
        {
            Console.WriteLine(JsonSerializer.Serialize(message));
        }
    }
}