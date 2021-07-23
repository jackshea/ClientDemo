using System;

namespace Infrastructure
{
    public class SpendTimer
    {
        public DateTime StartTime { get; private set; }
        public string Name { get; private set; }

        public SpendTimer(string name)
        {
            Name = name;
            Start();
        }

        /// 全局计时器
        public static SpendTimer Global { get; } = new SpendTimer("Global");

        /// 开始计时
        public void Start(string msg = "")
        {
            StartTime = DateTime.Now;
            Console.WriteLine($"{Name}开始计时。{msg}");
        }

        public void ShowSpend(string msg = "")
        {
            Console.WriteLine($"{Name}耗时{(DateTime.Now - StartTime).TotalMilliseconds}毫秒。{msg}");
        }
    }
}