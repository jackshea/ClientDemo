using MessagePack;
using System.Threading.Tasks;

namespace ClientDemo
{
    public class StructNetworkClient : ByteNetworkClient
    {
        public async Task SentMessage<T>(T msg)
        {
            var bytes = MessagePackSerializer.Serialize(msg);
            await SentMessage(bytes, 0, bytes.Length);
        }
    }
}