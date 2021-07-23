namespace Message
{
    public interface IMessageHandler<in T>
    {
        void Process(T message);
    }
}