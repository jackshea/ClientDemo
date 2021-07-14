namespace ClientDemo
{
    public interface IMessageHandler
    {
        void OnMessageReceived(byte[] message, int offset, int length);
    }
}