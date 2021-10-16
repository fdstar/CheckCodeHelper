using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper
{
    /// <summary>
    /// 校验码校验结果
    /// </summary>
    public enum VerificationResult
    {
        /// <summary>
        /// 校验成功
        /// </summary>
        [Description("成功")]
        Success = 0,
        /// <summary>
        /// 校验码已过期
        /// </summary>
        [Description("校验码已过期")]
        Expired = 31,
        /// <summary>
        /// 校验码不一致，校验失败
        /// </summary>
        [Description("校验失败")]
        Failed = 32,
        /// <summary>
        /// 已经达到了最大错误尝试次数，需重新发送新的校验码
        /// </summary>
        [Description("超出最大错误次数")]
        MaxErrorLimit = 33,
    }
}
