using System;
using System.Buffers;
using System.IO;
using MessagePack;

namespace ClientDemo
{
    public class MessageCoder
    {
        /// 编码
        /// 消息 = 消息长度（int）+ 消息类型（int）+ 消息体编码长度
        /// 消息长度 = 消息类型长度（int = 4） + 消息体编码长度
        public byte[] Encoder<T>(int msgType, T msgBody)
        {
            var msgBodyBytes = MessagePackSerializer.Serialize(msgBody);
            int msgBodyLength = sizeof(int) + msgBodyBytes.Length;
            int totalLength = sizeof(int) + msgBodyLength;
            var bytes = ArrayPool<byte>.Shared.Rent(totalLength);
            int offset = 0;
            var intBytes = BitConverter.GetBytes(msgBodyLength);
            Buffer.BlockCopy(intBytes, 0, bytes, offset, intBytes.Length);
            offset += intBytes.Length;
            intBytes = BitConverter.GetBytes(msgType);
            Buffer.BlockCopy(intBytes, 0, bytes, offset, intBytes.Length);
            offset += intBytes.Length;
            Buffer.BlockCopy(msgBodyBytes, 0, bytes, offset, msgBodyBytes.Length);
            return bytes;
        }

        //public int Decoder(byte[] bytes, Type type, object obj)
        //{
        //    var span = new Span<byte>(bytes, 0, 4);
        //    BitConverter.ToInt32(span)
        //    return MessagePackSerializer.Deserialize<T>(bytes);
        //}
    }
}