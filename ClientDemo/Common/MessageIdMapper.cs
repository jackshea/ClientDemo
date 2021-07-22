using System;
using System.Collections.Generic;
using Message;

namespace Common
{
    public class MessageIdMapper
    {
        public static MessageIdMapper Instance { get; } = new MessageIdMapper();

        private Dictionary<int, Type> id2Type = new Dictionary<int, Type>();
        private Dictionary<Type, int> type2Id = new Dictionary<Type, int>();

        private MessageIdMapper()
        {
            AddMap(123456,typeof(User));
        }

        public void AddMap(int id, Type type)
        {
            id2Type[id] = type;
            type2Id[type] = id;
        }

        public int GetId(Type type)
        {
            type2Id.TryGetValue(type, out var id);
            return id;
        }

        public Type GetType(int id)
        {
            id2Type.TryGetValue(id, out var type);
            return type;
        }
    }
}