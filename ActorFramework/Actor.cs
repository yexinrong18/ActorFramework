using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActorFramework
{
    /// <summary>
    /// 操作者
    /// </summary>
    public class Actor
    {
        /// <summary>
        /// 启动本Actor的Actor的消息队列
        /// </summary>
        protected MessagePriorityQueue _CallerQueue = new MessagePriorityQueue();

        /// <summary>
        /// 本Actor的消息队列
        /// </summary>
        protected MessagePriorityQueue _selfQueue = new MessagePriorityQueue();

        /// <summary>
        /// Actor在工作标记
        /// </summary>
        protected bool _working = false;

        protected BlockingCollection<bool> _pauseSyncQueue = new BlockingCollection<bool>();
        protected BlockingCollection<bool> _resumeSyncQueue = new BlockingCollection<bool>();

        /// <summary>
        /// 析构函数中停止线程
        /// </summary>
        ~Actor()
        {
            Messages.StopActor.SendStopMsg(ReadSelfEnqueue());
        }

        /// <summary>
        /// 准备启动ActorCore之前调用的方法
        /// </summary>
        protected virtual void PreStart() { }

        /// <summary>
        /// 停止之前调用的方法
        /// </summary>
        protected virtual void PreStop() { Console.WriteLine("Actor Stoped"); }

        /// <summary>
        /// Actor循环执行Message的核心部分
        /// </summary>
        protected virtual void ActorCore()
        {
            _working = true;
            Task.Factory.StartNew(() =>
            {
                while (_working)
                {
                    _selfQueue.PriorityDequeue()?.DoWork(this).Handle();
                }
            }, TaskCreationOptions.LongRunning).ContinueWith(t =>
            {
                PreStop();
            });
        }

        /// <summary>
        /// 启动Actor
        /// </summary>
        /// <returns></returns>
        public MessageEnqueue LaunchActor()
        {
            PreStart();
            ActorCore();
            return ReadSelfEnqueue();
        }

        public MessageEnqueue ReadSelfEnqueue()
        {
            return new MessageEnqueue(_selfQueue);
        }        

        /// <summary>
        /// Actor停止
        /// </summary>
        internal Error StopActor()
        {
            _working = false;
            return Error.NoError;
        }
    }
}
