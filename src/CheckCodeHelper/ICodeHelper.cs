using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper
{
    /// <summary>
    /// 业务校验码辅助接口
    /// </summary>
    public interface ICodeHelper
    {
        /// <summary>
        /// 校验码实际发送者
        /// </summary>
        ICodeSender Sender { get; }
        /// <summary>
        /// 校验码信息存储者
        /// </summary>
        ICodeStorage Storage { get; }
        /// <summary>
        /// 发送校验码
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <param name="code">校验码</param>
        /// <param name="effectiveTime">校验码有效时间范围</param>
        /// <param name="periodLimit">周期内最大允许发送配置，为null则表示无限制</param>
        /// <returns>校验码发送结果</returns>
        Task<SendResult> SendCodeAsync(string receiver, string bizFlag, string code, TimeSpan effectiveTime, PeriodLimit periodLimit);
        /// <summary>
        /// 验证校验码是否正确
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <param name="code">校验码</param>
        /// <param name="maxErrorLimit">最大允许错误次数</param>
        /// <param name="resetWhileRight">当验证通过时，是否重置周期次数限制，默认false</param>
        /// <returns>验证结果</returns>
        Task<VerificationResult> VerifyCodeAsync(string receiver, string bizFlag, string code, int maxErrorLimit, bool resetWhileRight = false);
        /// <summary>
        /// 获取校验码发送的CD时间，如果无CD时间，则返回<see cref="TimeSpan.Zero"/>
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <param name="periodLimit">周期内允许的发送配置，为null则表示无限制</param>
        /// <returns></returns>
        Task<TimeSpan> GetSendCDAsync(string receiver, string bizFlag, PeriodLimit periodLimit);
    }
}
