using System;
using System.Collections.Generic;
using System.Reflection;

namespace Message
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

        public void Init()
        {
            Add(typeof(User), new UserMsgHandler());
            Add(typeof(Hello), new HelloMsgHandler());
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