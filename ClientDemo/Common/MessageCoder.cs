using MessagePack;
using System;
using System.IO;

namespace Common
{
    public class MessageCoder
    {
        public int Encoder<T>(BinaryWriter bw, T msgBody)
        {
            var msgBodyBytes = MessagePackSerializer.Serialize(msgBody);
            int msgBodyLength = msgBodyBytes.Length;
            int totalLength = sizeof(int) + sizeof(int) + msgBodyLength;
            int msgTypeId = MessageIdMapper.Instance.GetId(typeof(T));
            bw.Write(msgTypeId);
            bw.Write(msgBodyLength);
            bw.Write(msgBodyBytes);
            return totalLength;
        }

        public object Decoder(BinaryReader br, out Type type)
        {
            var msgTypeId = br.ReadInt32();
            var msgBodyLength = br.ReadInt32();
            var msgBody = br.ReadBytes(msgBodyLength);
            type = MessageIdMapper.Instance.GetType(msgTypeId);
            return MessagePackSerializer.Deserialize(type, msgBody);
        }
    }
}