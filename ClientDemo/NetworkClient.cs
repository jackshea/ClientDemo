using Common;
using Message;
using System;
using System.Buffers;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ClientDemo
{
    public class NetworkClient
    {
        private TcpClient client;
        private NetworkStream ns;
        private BufferedStream bufferWriter;
        private BufferedStream bufferRead;
        private bool isStopRead;
        private BinaryWriter binWriter;
        private BinaryReader binReader;
        private MessageCoder coder = new MessageCoder();
        private MessageHandlerManager messageHandlerManager;

        public NetworkClient()
        {
            client = new TcpClient();
            messageHandlerManager = new MessageHandlerManager();
            messageHandlerManager.Init();
        }

        public async Task Connect(string host, int port)
        {
            await client.ConnectAsync(host, port);

            ns = client.GetStream();
            bufferWriter = new BufferedStream(ns, 65536);
            bufferRead = new BufferedStream(ns, 65536);
            binWriter = new BinaryWriter(bufferWriter);
            binReader = new BinaryReader(bufferRead);
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
            bufferRead.Close();
            bufferWriter.Close();
            ns.Close();
            client.Close();
        }

        public async Task<int> SentMessage<T>(T msg)
        {
            var msgLength = coder.Encode(binWriter, msg);
            binWriter.Flush();
            return await Task.FromResult(msgLength);
        }

        public object ReceiveMessage(out Type type)
        {
            var msg = coder.Decode(binReader, out var mgsType);
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
            Console.WriteLine($"client.Available 1 = {client.Available}");
            while(client.Available > 0)
            {
                var length = client.Available;
                var bytes = ArrayPool<byte>.Shared.Rent(length);
                var msg = coder.Decode(binReader, out var msgType);
                messageHandlerManager.Dispatch(msgType, msg);
                ArrayPool<byte>.Shared.Return(bytes);
                Console.WriteLine($"client.Available 2= {client.Available}");
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

        public void StopReceiveMessage()
        {
            isStopRead = true;
        }
    }
}