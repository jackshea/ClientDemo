using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ClientDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            List<Task> testTasks = new List<Task>();
            for (int i = 0; i < 1; i++)
            {
                var oneClientTest = OneClientTest();
                testTasks.Add(oneClientTest);
            }

            await Task.WhenAll(testTasks);
            Console.ReadLine();

        }

        static async Task OneClientTest()
        {
            var client = new ByteNetworkClient();
            await client.Connect("localhost", 10086);
            var stringMessageHandler = new StringMessageHandler();
            client.RegisterMessageHandler(stringMessageHandler);
            var startReceiveMessage = client.StartReceiveMessage(100);
            for (int i = 0; i < 10000; i++)
            {
                string message = $"Hello , this is {i + 1}th message\n";
                Console.Write("发送消息：" + message);
                var bytes = ArrayPool<byte>.Shared.Rent(1027);
                var length = Encoding.UTF8.GetBytes(message, bytes);
                await client.SentMessage(bytes, 0, length);
                //await Task.Delay(10);
            }
            client.StopReceiveMessage();
            await startReceiveMessage;
            await client.Close();
        }
    }
}
