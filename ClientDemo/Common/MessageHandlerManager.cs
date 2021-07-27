using Message;
using NLog;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Common
{
    public class MessageHandlerManager
    {
        private static ILogger log = LogManager.GetCurrentClassLogger();
        Dictionary<Type, object> dic = new Dictionary<Type, object>();
        private MethodInfo _method;

        public MessageHandlerManager()
        {
            _method = typeof(MessageHandlerManager).GetMethod("Process");
        }

        public void Add(Type type, object messageHandler)
        {
            dic.Add(type, messageHandler);
        }

        public void Init(Assembly asm)
        {
            var types = asm.GetTypes();
            StringBuilder sb = new StringBuilder();

            foreach (var type in types)
            {
                foreach (var @interface in type.GetInterfaces())
                {
                    if (@interface.Name == typeof(IMessageHandler<>).Name)
                    {
                        var genericType = @interface.GenericTypeArguments[0];
                        sb.AppendLine($"\tmsgType = {genericType.Name}, \thandlerType = {type.Name}");
                        object instance = Activator.CreateInstance(type);
                        Add(genericType, instance);
                        break;
                    }
                }
            }
            log.Debug("MessageHandlerManager add type. \n" + sb.ToString());
        }

        public void Process<T>(T msg)
        {
            if (!dic.TryGetValue(typeof(T), out var messageHandler))
            {
                return;
            }

            if (messageHandler is IMessageHandler<T> handle)
            {
                handle.Process(msg);
            }
        }

        public void Dispatch(Type type, object msg)
        {
            if (!dic.ContainsKey(type))
            {
                log.Error($"没有对应类型的消息处理器。 type =" + type.Name);
                return;
            }

            _method.MakeGenericMethod(type).Invoke(this, new object[] { msg });
        }
    }
}