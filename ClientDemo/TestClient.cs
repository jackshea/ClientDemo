using System;
using System.Buffers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SomeSample
{
    public class TestClient
    {
        private const int portNum = 10086;
        private const string hostName = "localhost";

        public static async Task<int> Run(String[] args)
        {
            try
            {
                var client = new TcpClient(hostName, portNum);

                NetworkStream ns = client.GetStream();
                ReceiveMsg(client, ns);
                DateTime start = DateTime.Now;
                int count = 10;
                for (int i = 0; i < count; i++)
                {
                    await SentMsg(ns, $"Hello {i}. \r\n");
                    await Task.Delay(1000);
                }

                var spend = DateTime.Now - start;
                Console.WriteLine($"发送{count}条数据，用时{spend.TotalMilliseconds:F2} 毫秒");
                Console.WriteLine($"每条数据用时{spend.TotalMilliseconds / count:F2} 毫秒");
                //byte[] bytes = ArrayPool<byte>.Shared.Rent(1024); 
                //Console.WriteLine($"CanRead = {ns.CanRead}");
                //int bytesRead = ns.Read(bytes, 0, bytes.Length);
                //Console.WriteLine(Encoding.ASCII.GetString(bytes, 0, bytesRead));
                await Task.Delay(1000);
                client.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return 0;
        }

        public static async Task<int> SentMsg(NetworkStream ns, string msg)
        {
            Encoding.UTF8.GetByteCount(msg);
            var sentBytes = ArrayPool<byte>.Shared.Rent(1024);
            int len = Encoding.ASCII.GetBytes(msg, sentBytes);
            Console.Write("发送消息：" + msg);
            await ns.WriteAsync(sentBytes, 0, len);
            ArrayPool<byte>.Shared.Return(sentBytes);
            return len;
        }

        public static async Task ReceiveMsg(TcpClient client, NetworkStream ns)
        {
            while (true)
            {
                if (client.Available > 0)
                {
                    var bytes = ArrayPool<byte>.Shared.Rent(client.Available);
                    await ns.ReadAsync(bytes);
                    Console.Write("收到消息：" + Encoding.ASCII.GetString(bytes));
                    ArrayPool<byte>.Shared.Return(bytes);
                }

                await Task.Delay(100);
            }
        }
    }
}