using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActorFramework.Messages
{
    /// <summary>
    /// 停止Actor的消息
    /// </summary>
    public class StopActor : Message
    {
        public override Error DoWork(Actor actor)
        {
            return actor.StopActor();
        }

        public static void SendStopMsg(MessageEnqueue queue, bool Emergency = true)
        {
            if (Emergency)
            {
                queue.EnqueueCritical(new StopActor());
            }
            else
            {
                queue.Enqueue(new StopActor());
            }
        }
    }
}
