using System.Collections.Concurrent;

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
        private MessagePriorityQueue _CallerQueue = new MessagePriorityQueue();

        /// <summary>
        /// 本Actor的消息队列
        /// </summary>
        private MessagePriorityQueue _selfQueue = new MessagePriorityQueue();

        /// <summary>
        /// Actor在工作标记
        /// </summary>
        private bool _working = false;

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
                    ErrorHandler(_selfQueue.PriorityDequeue()?.DoWork(this));
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

        /// <summary>
        /// 读取自身的队列
        /// </summary>
        /// <returns></returns>
        public MessageEnqueue ReadSelfEnqueue()
        {
            return new MessageEnqueue(_selfQueue);
        }        

        /// <summary>
        /// 清空自身队列里面的消息
        /// </summary>
        protected void FlushSelfEnqueue()
        {
            _selfQueue.Flush();
        }

        /// <summary>
        /// 读取调用者的队列
        /// </summary>
        /// <returns></returns>
        public MessageEnqueue ReadCallerQueue()
        {
            return new MessageEnqueue(_CallerQueue);
        }

        /// <summary>
        /// 错误处理
        /// </summary>
        /// <param name="err"></param>
        protected virtual void ErrorHandler(Error? err)
        {
            if (err != null && err.IsError)
            {
                Console.WriteLine("error code = " + err.Code + ", " + err.Message);
            }
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
