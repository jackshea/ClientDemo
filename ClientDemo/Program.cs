using Common;
using Message;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace ClientDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            MessageIdMapper.Instance.Init(Assembly.GetExecutingAssembly());
            await OneClientTest();
            Console.ReadLine();
        }

        static async Task OneClientTest()
        {
            var client = new NetworkClient();
            await client.Connect("127.0.0.1", 10087);

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
            for (int i = 0; i < 1000; i++)
            {
                user.Age = i;
                await client.SentMessage(user, false);
                await client.SentMessage(new Hello
                {
                    Greeting = $"Hello {i}, random = {random.Next(int.MaxValue)}"
                });
                //await Task.Delay(1);
            }
            await client.FlushAsync();
            await client.TryStopReceiveMessage(1);
            await startReceiveMessage;
            await client.Close();
        }
    }
}
