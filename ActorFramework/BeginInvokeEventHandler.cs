using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActorFramework
{
    public class BeginInvokeEventHandler<TEventArgs>
    {
        public EventHandler<TEventArgs> EventHandler { get; set; }

        public Task BeginInvoke(object sender, TEventArgs eventArgs, Action? callback, object @object)
        {
            return Task.Run(() => EventHandler?.Invoke(sender, eventArgs)).ContinueWith(t => callback);
        }
    }
}
