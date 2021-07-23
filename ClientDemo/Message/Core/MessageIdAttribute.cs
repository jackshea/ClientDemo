using System;

namespace Message
{
    /// 消息Id
    public class MessageIdAttribute : Attribute
    {
        public int Id { get; }

        public MessageIdAttribute(int id)
        {
            Id = id;
        }
    }
}