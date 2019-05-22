using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper.Samples
{
    /// <summary>
    /// 在控制台输出校验码
    /// </summary>
    public class ConsoleSender : ICodeSender
    {
        public ConsoleSender(IContentFormatter formatter)
        {
            this.Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        }
        public IContentFormatter Formatter { get; }

        public bool IsSupport(string receiver) => true;

        public Task<bool> Send(string receiver, string bizFlag, string code, TimeSpan effectiveTime)
        {
            var content = this.Formatter.GetContent(receiver, bizFlag, code, effectiveTime);
            Console.WriteLine("发送内容：{0}", content);
            return Task.FromResult(true);
        }
    }
}
