using System.Buffers;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ClientDemo
{
    public class ByteNetworkClient
    {
        private TcpClient client;
        private NetworkStream ns;
        private List<IMessageHandler> messageHandlers;
        private bool isStopRead;

        public ByteNetworkClient()
        {
            client = new TcpClient();
            messageHandlers = new List<IMessageHandler>();
        }

        public async Task Connect(string host, int port)
        {
            await client.ConnectAsync(host, port);
            ns = client.GetStream();
        }

        public void Close()
        {
            StopReceiveMessage();
            messageHandlers.Clear();
            ns.Flush();
            ns.Close();
            client.Close();
        }

        public async Task SentMessage(byte[] bytes, int offset, int length)
        {
            await ns.WriteAsync(bytes, offset, length);
        }

        public async Task<byte[]> ReadMessage()
        {
            if (client.Available > 0)
            {
                var bytes = new byte[client.Available];
                await ns.ReadAsync(bytes);
                return bytes;
            }
            return new byte[0];
        }

        public void RegisterMessageHandler(IMessageHandler messageHandler)
        {
            messageHandlers.Add(messageHandler);
        }

        public void UnregisterMessageHandler(IMessageHandler messageHandler)
        {
            messageHandlers.Remove(messageHandler);
        }

        public async Task TryReceiveMessage()
        {
            if (client.Available > 0)
            {
                var length = client.Available;
                var bytes = ArrayPool<byte>.Shared.Rent(length);
                await ns.ReadAsync(bytes);
                foreach (var messageHandler in messageHandlers)
                {
                    messageHandler.OnMessageReceived(bytes, 0, length);
                }
                ArrayPool<byte>.Shared.Return(bytes);
            }
        }

        public async void StartReceiveMessage(int intervalMs)
        {
            isStopRead = false;
            while (!isStopRead)
            {
                await TryReceiveMessage();
                await Task.Delay(intervalMs);
            }
        }

        public void StopReceiveMessage()
        {
            isStopRead = true;
        }
    }
}