using ActorFramework.Messages;
using System.Collections.Concurrent;

namespace ActorFramework
{
    public enum TimeDelayOptions
    {
        SendNextCopyNow,
        SkipNextScheduledCopy,
        SendNextCopyNowAndStopAllFurtherCopies,
        StopAllFurtherCopies
    }

    /// <summary>
    /// 间隔时间发送消息
    /// </summary>
    public class TimeDelaySendMessage
    {
        #region fields
        protected Message _msg;

        protected int _residueCopies;

        protected BlockingCollection<TimeDelayOptions> _controlQueue = new BlockingCollection<TimeDelayOptions>();

        protected MessagePriority _priority = MessagePriority.Normal;

        protected MessageEnqueue _enqueue;
        #endregion

        /// <summary>
        /// 定时循环发送消息
        /// </summary>
        /// <param name="enqueue">执行定时循环的操作者队列</param>
        /// <param name="msg">消息</param>
        /// <param name="MillisecondToWait">定时时间，单位ms</param>
        /// <param name="copies">重复次数</param>
        /// <param name="priority">优先级</param>
        public TimeDelaySendMessage(MessageEnqueue enqueue, Message msg,
            int millisecondToWait = -1, int copies = 0, MessagePriority priority = MessagePriority.Normal)
        {
            _enqueue = enqueue;
            _msg = msg;
            Copies = copies;
            MillisecondToWait = millisecondToWait;
            _priority = priority;
        }

        #region properties
        public int Copies { get; set; }

        public bool IsWorking { get; set; }

        public int MillisecondToWait { get; set; }
        #endregion

        #region method
        public void LaunchActor()
        {
            IsWorking = true;
            _residueCopies = Copies;
            Task.Factory.StartNew(() =>
            {
                while (IsWorking || _residueCopies == 0)
                {
                    TimeDelayOptions options;
                    if (_controlQueue.TryTake(out options, MillisecondToWait))
                    {
                        switch (options)
                        {
                            case TimeDelayOptions.SendNextCopyNow:
                                Message.SendMessage(_enqueue, _msg, _priority);
                                break;
                            case TimeDelayOptions.SkipNextScheduledCopy:
                                break;
                            case TimeDelayOptions.SendNextCopyNowAndStopAllFurtherCopies:
                                Message.SendMessage(_enqueue, _msg, _priority);
                                IsWorking = false;
                                break;
                            case TimeDelayOptions.StopAllFurtherCopies:
                                IsWorking = false;
                                break;
                            default:
                                break;
                        }
                    }
                    // 超时，通过设定超时时间来实现定时发送指令
                    else
                    {
                        Message.SendMessage(_enqueue, _msg, _priority);
                    }
                    // 判断设定重复次数，满足就退出线程
                    _residueCopies--;
                }
            });
        }

        public void Control(TimeDelayOptions options)
        {
            _controlQueue.TryAdd(options);
        }
        #endregion
    }
}
