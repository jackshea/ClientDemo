using System;
using System.Text;

namespace ClientDemo
{
    public class StringMessageHandler : IMessageHandler
    {
        public void OnMessageReceived(byte[] message, int offset, int length)
        {
            Console.Write("收到消息：" + Encoding.UTF8.GetString(message, offset, length));
        }
    }
}