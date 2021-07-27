using Common;
using Message;
using System;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using Infrastructure;
using NLog;
using NLog.LayoutRenderers;

namespace ClientDemo
{
    class Program
    {
        private static ILogger log = LogManager.GetCurrentClassLogger();
        static async Task Main(string[] args)
        {
            var startTime = DateTime.Now.ToString("s");
            LayoutRenderer.Register("startTime", (logEvent) => startTime);
            var log = LogManager.GetCurrentClassLogger();
            log.Info("init Main.");
            MessageIdMapper.Instance.Init(Assembly.GetExecutingAssembly());
            try
            {
                await OneClientTest();
            }
            catch (Exception e)
            {
                log.Error(e);
            }

            Console.ReadLine();
        }

        static async Task OneClientTest()
        {
            var client = new NetworkClient();
            await client.Connect("127.0.0.1", 10087);
            if (!client.IsConnected())
            {
                log.Error("无法接连到服务器！");
                return;
            }
            var user = new User
            {
                Age = 10,
                FirstName = "Jack",
                LastName = "Shea",
                Address = new Address
                {
                    City = "Xiamen",
                    Country = "China",
                    State = "Fujian"
                }
            };

            await client.SentMessage(user);
            var random = new Random(0);
            var startReceiveMessage = client.StartReceiveMessage(100);
            var spendTimer = new SpendTimer("发送网络消息");
            for (int i = 0; i < 1000; i++)
            {
                user.Age = i;
                await client.SentMessage(user, false);
                await client.SentMessage(new Hello
                {
                    Greeting = $"Hello {i}, random = {random.Next(int.MaxValue)}"
                });
                await Task.Delay(100);
            }
            await client.FlushAsync();
            spendTimer.ShowSpend("发送完成");
            await client.TryStopReceiveMessage(1);
            await startReceiveMessage;
            spendTimer.ShowSpend("停止接收");
            await client.Close();
        }
    }
}
