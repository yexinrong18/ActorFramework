using System.Collections.Concurrent;

namespace ActorFramework.Messages
{
    /// <summary>
    /// ActorFrameWork消息
    /// </summary>
    public abstract class Message
    {
        /// <summary>
        /// 消息处理逻辑
        /// </summary>
        public abstract Error DoWork(Actor actor);

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="q">传送消息的队列</param>
        /// <param name="msg">消息数据</param>
        /// <param name="priority">优先级</param>
        public static void SendMessage(MessageEnqueue q, Message msg, MessagePriority priority = MessagePriority.Normal)
        {
            q?.Enqueue(msg, priority);
        }
    }

    /// <summary>
    /// 需要回复的消息
    /// </summary>
    /// <typeparam name="T">回复消息的内容</typeparam>
    public abstract class ReplyMessage<T> : Message
    {
        /// <summary>
        /// 回复队列，使用堵塞集合是为了参数堵塞延迟效果。
        /// </summary>
        protected BlockingCollection<T> ReplyMessageQueue;

        /// <summary>
        /// 需要回复的消息执行核心部分
        /// </summary>
        /// <param name="actor"></param>
        /// <returns>回复消息的内容</returns>
        public abstract T DoCore(Actor actor);

        public override Error DoWork(Actor actor)
        {
            ReplyMessageQueue.TryAdd(DoCore(actor));
            return Error.NoError;
        }

        /// <summary>
        /// 发送需要回复的消息
        /// </summary>
        /// <param name="q">操作作者消息队列</param>
        /// <param name="msg">消息内容</param>
        /// <param name="priority">优先级</param>
        /// <param name="timeout">超时，-1为无穷</param>
        /// <returns></returns>
        public static T SendMessageAndWaitForResponse(MessageEnqueue q, ReplyMessage<T> msg, MessagePriority priority = MessagePriority.Normal, int timeout = -1)
        {
            msg.ReplyMessageQueue = new BlockingCollection<T>();
            // 发送消息
            SendMessage(q, msg, priority);
            T itme;
            // 等待执行该消息操作者返回，也就是有消息进入回复消息的队列，这里出列了就是说明收到回复了。
            msg.ReplyMessageQueue.TryTake(out itme, timeout);
            return itme;
        }
    }



    /// <summary>
    /// Actor停止，最后一次发送消息给调用者(父Actor)
    /// </summary>
    public class LastAskMessage : Message
    {
        public override Error DoWork(Actor actor)
        {
            return Error.NoError;
        }
    }
}
