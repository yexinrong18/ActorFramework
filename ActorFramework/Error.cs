using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActorFramework
{
    public class Error
    {
        public bool IsError { get; set; }
        public string Message { get; set; } = "";
        public int Code { get; set; }

        public static Error NoError = new Error() { IsError = false };
    }
}
