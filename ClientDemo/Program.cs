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
            await OneClientTest();
            Console.ReadLine();
        }

        static async Task OneClientTest()
        {
            var client = new ByteNetworkClient();
            await client.Connect("localhost", 10086);
            var stringMessageHandler = new StringMessageHandler();
            client.RegisterMessageHandler(stringMessageHandler);
            var startReceiveMessage = client.StartReceiveMessage(100);
            for (int i = 0; i < 100; i++)
            {
                string message = $"Hello , this is {i + 1}th message\n";
                await client.SetMessage(message);
                //await Task.Delay(10);
            }

            await Task.Delay(10000);
            await client.SetMessage("Hello, again!");
            client.StopReceiveMessage();
            await startReceiveMessage;
            await client.Close();
        }
    }
}
