using MessagePack;

namespace Message
{
    [MessageId(2)]
    [MessagePackObject]
    public class Hello
    {
        [Key(0)]
        public string Greeting { get; set; }
    }
}