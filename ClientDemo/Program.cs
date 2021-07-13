using System;
using System.Threading.Tasks;
using SomeSample;

namespace ClientDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await TestClient.Run(args);
            Console.ReadLine();
        }
    }
}
