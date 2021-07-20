using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //var clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            //Console.WriteLine("Connecting to port 10086");

            //clientSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10086));
            //var stream = new NetworkStream(clientSocket);
            //Console.InputEncoding = Encoding.UTF8;
            //Console.OutputEncoding = Encoding.UTF8;
            //await Console.OpenStandardInput().CopyToAsync(stream);

            await OneClientTest();
            Console.ReadLine();
        }

        static async Task OneClientTest()
        {
            var client = new ByteNetworkClient();
            await client.Connect("127.0.0.1", 10087);
            var stringMessageHandler = new StringMessageHandler();
            client.RegisterMessageHandler(stringMessageHandler);
            var startReceiveMessage = client.StartReceiveMessage(100);
            for (int i = 0; i < 100; i++)
            {
                string message = $"Hello , this is {i + 1}th message\n";
                await client.SentMessage(message);
                await Task.Delay(1000);
            }

            await Task.Delay(10000);
            await client.SentMessage("Hello, again!");
            client.StopReceiveMessage();
            await startReceiveMessage;
            await client.Close();
        }
    }
}
