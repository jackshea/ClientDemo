using System;
using System.Collections.Generic;
using System.Reflection;
using Message;

namespace Common
{
    public class MessageHandlerManager
    {
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
            foreach (var type in types)
            {
                foreach (var @interface in type.GetInterfaces())
                {
                    if (@interface.Name == typeof(IMessageHandler<>).Name)
                    {
                        Console.WriteLine(type.FullName);
                        var genericType = @interface.GenericTypeArguments[0];
                        object instance = Activator.CreateInstance(type);
                        Add(genericType, instance);
                        break;
                    }
                }
            }
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
                return;
            }

            _method.MakeGenericMethod(type).Invoke(this, new object[] { msg });
        }
    }
}