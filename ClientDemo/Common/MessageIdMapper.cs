using Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NLog;

namespace Common
{
    public class MessageIdMapper
    {
        private static ILogger log = LogManager.GetCurrentClassLogger();
        public static MessageIdMapper Instance { get; } = new MessageIdMapper();

        private Dictionary<int, Type> id2Type = new Dictionary<int, Type>();
        private Dictionary<Type, int> type2Id = new Dictionary<Type, int>();

        private MessageIdMapper()
        {
            AddMapper(123456, typeof(User));
        }

        public void Init(Assembly asm)
        {
            var types = asm.GetTypes();
            foreach (var type in types)
            {
                var attributes = type.GetCustomAttributes(typeof(MessageIdAttribute));
                if (!attributes.Any())
                {
                    continue;
                }

                if (!type.IsClass)
                {
                    continue;
                }

                foreach (var attribute in attributes)
                {
                    if (attribute is MessageIdAttribute msgId)
                    {
                        var id = msgId.Id;
                        AddMapper(id, type);
                    }
                }
            }

            StringBuilder sb = new StringBuilder();
            var ids = id2Type.Keys.ToList();
            ids.Sort();
            foreach (var id in ids)
            {
                sb.AppendLine($"\tid = {id}, \tType = {id2Type[id]}");
            }
            log.Debug("MessageIdMapper:\n" + sb);
        }

        public void AddMapper(int id, Type type)
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