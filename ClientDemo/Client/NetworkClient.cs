using Common;
using Message;
using System;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using NLog;

namespace Client
{
    public class NetworkClient
    {
        private static ILogger log = LogManager.GetCurrentClassLogger();
        private TcpClient client;
        private NetworkStream ns;
        private bool isStopRead;
        private BinaryWriter binWriter;
        private BinaryReader binReader;
        private MessageCoder coder = new MessageCoder();
        private MessageHandlerManager messageHandlerManager;
        private DateTime lastAlive;

        public NetworkClient()
        {
            client = new TcpClient();
            messageHandlerManager = new MessageHandlerManager();
            messageHandlerManager.Init(Assembly.GetExecutingAssembly());
        }

        public async Task Connect(string host, int port)
        {
            try
            {
                await client.ConnectAsync(host, port);
                ns = client.GetStream();
                binWriter = new BinaryWriter(ns);
                binReader = new BinaryReader(ns);
            }
            catch (Exception e)
            {
                log.Fatal(e);
            }
        }

        public bool IsConnected()
        {
            return client.Connected;
        }

        public async Task Close()
        {
            await ns.FlushAsync();
            await TryReceiveMessage();
            binReader.Close();
            binWriter.Close();
            ns.Close();
            client.Close();
        }

        public async Task<int> SentMessage<T>(T msg, bool autoFlush = true)
        {
            var msgLength = coder.Encode(binWriter, msg);
            if (autoFlush)
            {
                binWriter.Flush();
            }

            lastAlive = DateTime.Now;
            return await Task.FromResult(msgLength);
        }

        public async Task FlushAsync()
        {
            await ns.FlushAsync();
        }

        public object ReceiveMessage(out Type type)
        {
            coder.Decode(binReader, out var mgsType, out var msg);
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

        public async Task TryReceiveMessage()
        {
            var available = client.Available;
            while (available > 0)
            {
                available -= coder.Decode(binReader, out var msgType, out var msg);
                messageHandlerManager.Dispatch(msgType, msg);
                lastAlive = DateTime.Now;
            }
            await Task.CompletedTask;
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

        public async Task TryStopReceiveMessage(double idleTime)
        {
            while ((DateTime.Now - lastAlive).TotalSeconds < idleTime)
            {
                await Task.Delay(100);
            }

            isStopRead = true;
        }
    }
}