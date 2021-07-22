using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Common;

namespace ClientDemo
{
    public class NetworkClient
    {
        private TcpClient client;
        private NetworkStream ns;
        private BufferedStream bs;
        private List<IMessageHandler> messageHandlers;
        private bool isStopRead;
        private BinaryWriter binWriter;
        private BinaryReader binReader;
        private MessageCoder coder = new MessageCoder();

        public NetworkClient()
        {
            client = new TcpClient();
            messageHandlers = new List<IMessageHandler>();
        }

        public async Task Connect(string host, int port)
        {
            await client.ConnectAsync(host, port);

            ns = client.GetStream();
            bs = new BufferedStream(ns);
            binWriter = new BinaryWriter(bs);
            binReader = new BinaryReader(bs);
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
            binWriter.Close();
            binReader.Close();
            client.Close();
        }

        public async Task<int> SentMessage<T>(T msg)
        {
            var msgLength = coder.Encoder(binWriter, msg);
            binWriter.Flush();
            return await Task.FromResult(msgLength);
        }

        public object ReceiveMessage(out Type type)
        {
            var msg = coder.Decoder(binReader, out var mgsType);
            type = mgsType;
            return msg;
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