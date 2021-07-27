using System;
using NLog;

namespace Infrastructure
{
    public class SpendTimer
    {
        private static ILogger log = LogManager.GetCurrentClassLogger();
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
            log.Debug($"{Name}开始计时。{msg}");
        }

        public void ShowSpend(string msg = "")
        {
            log.Debug($"{Name}耗时{(DateTime.Now - StartTime).TotalMilliseconds}毫秒。{msg}");
        }
    }
}