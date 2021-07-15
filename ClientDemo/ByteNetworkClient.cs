using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
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
            client.Client.IOControl(IOControlCode.KeepAliveValues, GetKeepAliveData(), null);
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        }

        private byte[] GetKeepAliveData()
        {
            uint dummy = 0;
            byte[] inOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
            BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes((uint)3000).CopyTo(inOptionValues, Marshal.SizeOf(dummy));//keep-alive间隔
            BitConverter.GetBytes((uint)500).CopyTo(inOptionValues, Marshal.SizeOf(dummy) * 2);// 尝试间隔
            return inOptionValues;
        }

        public async Task Connect(string host, int port)
        {
            await client.ConnectAsync(host, port);
            ns = client.GetStream();
        }

        public bool IsConnected()
        {
            return client.Connected;
        }

        public async Task Close()
        {
            await ns.FlushAsync();
            await TryReceiveMessage();
            messageHandlers.Clear();
            ns.Close();
            client.Close();
        }

        public async Task SentMessage(byte[] bytes, int offset, int length)
        {
            await ns.WriteAsync(bytes, offset, length);
        }

        public async Task SetMessage(string message)
        {
            Console.Write("发送消息：" + message);
            var bytes = ArrayPool<byte>.Shared.Rent(1027);
            var length = Encoding.UTF8.GetBytes(message, bytes);
            await SentMessage(bytes, 0, length);
            ArrayPool<byte>.Shared.Return(bytes);
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

        public async Task StartReceiveMessage(int intervalMs)
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