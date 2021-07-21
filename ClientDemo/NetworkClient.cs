using MessagePack;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ClientDemo
{
    public class NetworkClient
    {
        private TcpClient client;
        private NetworkStream ns;
        private BufferedStream bs;
        private List<IMessageHandler> messageHandlers;
        private bool isStopRead;
        //private MemoryStream ms;
        public NetworkClient()
        {
            client = new TcpClient();
            messageHandlers = new List<IMessageHandler>();
            //ms = new MemoryStream();
        }

        public async Task Connect(string host, int port)
        {
            await client.ConnectAsync(host, port);

            ns = client.GetStream();
            bs = new BufferedStream(ns);
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
            bs.Close();
            ns.Close();
            client.Close();
        }

        public async Task SentMessage<T>(T msg)
        {
            bs.Write(BitConverter.GetBytes(123456));
            await MessagePackSerializer.SerializeAsync(bs, msg);
            await bs.FlushAsync();
        }

        public async Task<T> ReceiveMessage<T>()
        {
            var bytes = ArrayPool<byte>.Shared.Rent(1024);
            //ns.Read(bytes, 0, 1024);
            //int msgType = BitConverter.ToInt32(bytes);

            //ns.Seek(0, SeekOrigin.Begin);
            T mesBody = default;
            try
            {
                mesBody = MessagePackSerializer.Deserialize<T>(ns);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            ArrayPool<byte>.Shared.Return(bytes);
            return await Task.FromResult(mesBody);
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