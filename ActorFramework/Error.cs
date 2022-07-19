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

        public Error() { }

        public Error(int code, string message)
        {
            IsError = true;
            Code = code;
            Message = message;
        }

        /// <summary>
        /// 错误处理
        /// </summary>
        public void Handle()
        {
            if (IsError)
            {
                Console.WriteLine(Code + Message);
            }
        }

        /// <summary>
        /// 创建try catch到的错误
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Error CatchError(string message)
        {
            return new Error(-2, message);
        }

        public static Error NoError = new Error() { IsError = false };
        public static Error InvalidMessage = new Error(-1, "该消息不属于本Actor可执行的消息");
    }
}
