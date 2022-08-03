using System.Collections.Concurrent;

namespace ActorFramework
{
    public enum MessagePriority : int
    {
        Low = 3,
        Normal = 2,
        High = 1,
    }

    /// <summary>
    /// 有优先级的消息队列
    /// </summary>
    public class MessagePriorityQueue
    {
        public string Name { get; set; } = "";

        /// <summary>
        /// 等待时候的队列，当_buffers里面没有消息处于等待时候，消息都进该队列
        /// </summary>
        protected BlockingCollection<Messages.Message> _waitMsgQueue = new BlockingCollection<Messages.Message>();

        /// <summary>
        /// 等待队列中的消息优先级
        /// </summary>
        protected BlockingCollection<int> _waitPriorQueue = new BlockingCollection<int>();

        /// <summary>
        /// 等待队列中等待消息数
        /// </summary>
        protected volatile int _waiting = 0;

        /// <summary>
        /// 消息队列同步锁
        /// </summary>
        protected object _syncObj = new object();

        /// <summary>
        /// 缓存队列， 分层0-3等级，3-1对应MessagePriority的优先级，0是最高。
        /// 选用Queue的原因是因为Queue效率最高，效率由大到小Queue > ConcurrentQueue > BlockingCollection
        /// </summary>
        protected Queue<Messages.Message>[] _buffers = new Queue<Messages.Message>[]
        {
            new Queue<Messages.Message>(),
            new Queue<Messages.Message>(),
            new Queue<Messages.Message>(),
            new Queue<Messages.Message>()
        };

        public MessagePriorityQueue()
        {

        }

        /// <summary>
        /// 消息传入队列
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="priority">优先级</param>
        public void PriorityEnqueue(Messages.Message msg, int priority)
        {
            //Console.WriteLine(Name + "传入消息， Waiting = " + _waiting);
            // 如果队列中没有等待，直接把消息进入缓存队列
            lock (_syncObj)
            {
                if (_waiting == 0)
                {
                    _buffers[priority].Enqueue(msg);
                    //Console.WriteLine(Name + "传入缓存队列消息， Waiting = " + _waiting);
                }
                // 等待队列
                else
                {
                    _waitMsgQueue.TryAdd(msg);
                    //Console.WriteLine(Name + "传入等待队列消息， Waiting = " + _waiting);
                    // 可以不需要原子操作
                    _waiting--;
                }
            }
        }
        /// <summary>
        /// 消息传入队列
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="priority">优先级</param>
        public void PriorityEnqueue(Messages.Message msg, MessagePriority priority)
        {
            PriorityEnqueue(msg, (int)priority);
        }
        //internal void PriorityEnqueue(object message, MessagePriority priority)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// 消息出列
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public Messages.Message PriorityDequeue(int timeout = -1)
        {
            // 缓存队列里面有消息，从缓存队列里面出列
            //Console.WriteLine(Name + "检查缓存队列， Waiting = " + _waiting);
            lock (_syncObj)
            {
                if (_waiting == 0)
                {
                    foreach (var queue in _buffers)
                    {
                        // 只要有消息出列，就可以返回了。
                        //if (queue.TryTake(out message, 0)) { return message; } // 这样效率低
                        if (queue.Count > 0)
                        {
                            return queue.Dequeue();
                        }
                    }
                }
                // 可以不需要原子操作
                _waiting++;
            }

            // 再检查一次缓存队列里面是否有消息， 确定没有再检查等待队列
            //Console.WriteLine(Name + "再检查一次缓存队列， Waiting = " + _waiting);
            //foreach (var queue in _buffers)
            //{
            //    if (queue.Count > 0)
            //    {
            //        queue.TryTake(out message, 0);
            //        Console.WriteLine(Name + "再检查中获取消息， Waiting = " + _waiting);
            //        // 恢复等待队列里面无等待消息
            //        _waiting--;
            //        return message;
            //    }
            //}
            //Console.WriteLine(Name + "检查等待队列， Waiting = " + _waiting);
            // 缓存队列里面没有消息，从等待队列中出列
            Messages.Message message;
            if (_waitMsgQueue.TryTake(out message, timeout))
            {
                // 等待队列的消息已经执行，下次轮询可以从消息队列开始，恢复等待队列里面无等待消息
                //Console.WriteLine(Name + "已经从等待队列获取消息， Waiting = " + _waiting);
                return message;
            }
            // 如果设置超时时间，超时后_waiting需要恢复
            _waiting--;
            Console.WriteLine("超时");
            // 超时
            return null;
        }

        /// <summary>
        /// 清空队列
        /// </summary>
        /// <returns>队列中移除的消息</returns>
        public List<Messages.Message> Flush()
        {
            List<Messages.Message> listMsg = new List<Messages.Message>();
            // 情况缓存队列
            foreach (var queue in _buffers)
            {
                while (queue.Count > 0)
                {
                    listMsg.Add(queue.Dequeue());
                }
            }
            return listMsg;
        }

        /// <summary>
        /// 释放队列
        /// </summary>
        /// <returns>队列中移除的消息</returns>
        public List<Messages.Message> ReleasePriorityQueue()
        {
            return Flush();
        }
    }

    /// <summary>
    /// 传入队列类
    /// </summary>
    public class MessageEnqueue
    {
        public string Name
        {
            get => _messagePriorityQueue?.Name;
            set => _messagePriorityQueue.Name = value;
        }

        /// <summary>
        /// 队列
        /// </summary>
        protected MessagePriorityQueue _messagePriorityQueue;

        public MessageEnqueue(MessagePriorityQueue queue)
        {
            _messagePriorityQueue = queue;
        }

        /// <summary>
        /// 进列
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="priority">优先级</param>
        internal void Enqueue(Messages.Message msg, MessagePriority priority = MessagePriority.Normal)
        {
            _messagePriorityQueue.PriorityEnqueue(msg, (int)priority);
        }

        /// <summary>
        /// 最优先进列
        /// </summary>
        /// <param name="msg"></param>
        internal void EnqueueCritical(Messages.Message msg)
        {
            // 0是最高优先级
            _messagePriorityQueue.PriorityEnqueue(msg, 0);
        }
    }
}
