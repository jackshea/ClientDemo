using System;

namespace Message
{
    /// 消息处理器
    public class MessageHandlerAttribute : Attribute
    {
        public int Id { get; }

        public MessageHandlerAttribute(int id)
        {
            Id = id;
        }
    }
}