using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CheckCodeHelper.Sender.Sms
{
    /// <summary>
    /// 通过短信发送验证码
    /// </summary>
    public class SmsSender : ICodeSender
    {
        /// <summary>
        /// 通过短信发送验证码
        /// </summary>
        /// <param name="formatter">验证码内容模板</param>
        /// <param name="sms">短信发送接口</param>
        public SmsSender(IContentFormatter formatter, ISms sms)
        {
            this.Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            this.Sms = sms ?? throw new ArgumentNullException(nameof(sms));
        }
        /// <summary>
        /// 发送验证码内容模板
        /// </summary>
        public IContentFormatter Formatter { get; }
        /// <summary>
        /// 短信发送接口
        /// </summary>
        public ISms Sms { get; }
        /// <summary>
        /// 判断接收者是否符合发送条件，目前是宽松判断，即正则 ^1\d{10}$
        /// </summary>
        /// <param name="receiver"></param>
        /// <returns></returns>
        public virtual bool IsSupport(string receiver)
        {
            return !string.IsNullOrWhiteSpace(receiver)
                && Regex.IsMatch(receiver, @"^1\d{10}$");
        }
        /// <summary>
        /// 发送校验码信息
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <param name="code">校验码</param>
        /// <param name="effectiveTime">校验码有效时间范围</param>
        /// <returns></returns>
        public virtual async Task<bool> SendAsync(string receiver, string bizFlag, string code, TimeSpan effectiveTime)
        {
            var content = this.Formatter.GetContent(receiver, bizFlag, code, effectiveTime);
            var ret = await this.Sms.SendMessageAsync(receiver, content).ConfigureAwait(false);
            return ret;
        }
    }
}
