using System.Diagnostics;

namespace ActorFramework
{
    /// <summary>
    /// 帧率计算器
    /// </summary>
    public class FrameRateCalculator
    {
        /// <summary>
        /// 计时器
        /// </summary>
        protected Stopwatch _stopwatch = new Stopwatch();

        /// <summary>
        /// 帧数累加
        /// </summary>
        protected int _framesAddUp = 0;

        /// <summary>
        /// 更新周期，单位ms
        /// </summary>
        public int UpdateCycle { get; set; } = 1000;

        /// <summary>
        /// 设定事件更新一次帧率的事件
        /// </summary>
        public EventHandler<double> RefreshFrameRateEventHandler { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="UpdateCycle">更新周期，单位ms</param>
        public FrameRateCalculator(int updateCycle = 1000)
        {
            UpdateCycle = updateCycle;
        }

        /// <summary>
        /// 帧率计算器启动
        /// </summary>
        public void Start()
        {
            _stopwatch.Start();
            _framesAddUp = 0;
        }

        /// <summary>
        /// 计算一帧
        /// </summary>
        public void ExecuteOneFrame()
        {
            _framesAddUp++;
            _stopwatch.Stop();
            if (_stopwatch.ElapsedMilliseconds > UpdateCycle)
            {
                RefreshFrameRateEventHandler?.BeginInvoke(this, _framesAddUp * 1000.0 / _stopwatch.ElapsedMilliseconds, null, null);
                _stopwatch.Restart();
                _framesAddUp = 0;
            }
            else
            {
                _stopwatch.Start();
            }
        }

        /// <summary>
        /// 帧率计算器停止
        /// </summary>
        public void Stop()
        {
            _stopwatch.Stop();
        }
    }
}
