using MessagePack;
using System;
using System.IO;

namespace Common
{
    public class MessageCoder
    {
        private const int SizeOfMsgType = sizeof(int); // 消息类型本身的长度
        public int Encode<T>(BinaryWriter bw, T msgBody)
        {
            int msgTypeId = MessageIdMapper.Instance.GetId(typeof(T));
            var msgBodyBytes = MessagePackSerializer.Serialize(msgBody);
            int msgLength = SizeOfMsgType + msgBodyBytes.Length;

            bw.Write(msgLength);
            bw.Write(msgTypeId);
            bw.Write(msgBodyBytes);
            return msgLength;
        }

        public object Decode(BinaryReader br, out Type type)
        {
            var msgLength = br.ReadInt32();
            var msgTypeId = br.ReadInt32();
            var msgBody = br.ReadBytes(msgLength - SizeOfMsgType);
            type = MessageIdMapper.Instance.GetType(msgTypeId);
            return MessagePackSerializer.Deserialize(type, msgBody);
        }
    }
}