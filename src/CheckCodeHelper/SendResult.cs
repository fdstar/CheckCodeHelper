using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper
{
    /// <summary>
    /// 校验码发送结果
    /// </summary>
    public enum SendResult
    {
        /// <summary>
        /// 发送成功
        /// </summary>
        [Description("成功")]
        Success = 0,
        /// <summary>
        /// 超出最大发送次数
        /// </summary>
        [Description("超出最大发送次数")]
        MaxSendLimit = 11,
        /// <summary>
        /// 发送失败，指<see cref="ICodeSender"/>的发送结果为false
        /// </summary>
        [Description("发送失败")]
        FailInSend = 12,
        /// <summary>
        /// 无法发送，<see cref="ICodeSender.IsSupport(string)"/>结果为false
        /// </summary>
        [Description("无法发送")]
        NotSupprot = 13,
        /// <summary>
        /// 发送间隔时间过短
        /// </summary>
        [Description("发送间隔时间过短")]
        IntervalLimit = 14,
    }
}
