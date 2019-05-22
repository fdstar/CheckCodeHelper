using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper
{
    /// <summary>
    /// 业务校验码辅助接口实现
    /// </summary>
    public class CodeHelper : ICodeHelper
    {
        /// <summary>
        /// 基于接口实现，可依赖注入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="storage"></param>
        public CodeHelper(ICodeSender sender, ICodeStorage storage)
        {
            this.Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            this.Storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }
        /// <summary>
        /// 校验码实际发送者
        /// </summary>
        public ICodeSender Sender { get; }
        /// <summary>
        /// 校验码信息存储者
        /// </summary>
        public ICodeStorage Storage { get; }
        /// <summary>
        /// 发送校验码
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <param name="code">校验码</param>
        /// <param name="effectiveTime">校验码有效时间范围</param>
        /// <param name="maxSendLimit">周期内最大允许发送配置，为null则表示无限制</param>
        public async Task<SendResult> SendCode(string receiver, string bizFlag, string code, TimeSpan effectiveTime, PeriodLimit maxSendLimit)
        {
            var result = SendResult.NotSupprot;
            if (this.Sender.IsSupport(receiver))
            {
                result = SendResult.MaxSendLimit;
                bool canSend = maxSendLimit == null;
                int sendTimes = 0;
                if (!canSend)
                {
                    sendTimes = await this.Storage.GetAreadySendTimes(receiver, bizFlag).ConfigureAwait(false);
                    canSend = sendTimes < maxSendLimit.MaxLimit;
                }
                if (canSend)
                {
                    result = SendResult.FailInSend;
                    if (await this.Sender.Send(receiver, bizFlag, code, effectiveTime).ConfigureAwait(false)
                        && await this.Storage.SetCode(receiver, bizFlag, code, effectiveTime).ConfigureAwait(false))
                    {
                        result = SendResult.Success;
                        if (maxSendLimit != null)
                        {
                            if (sendTimes == 0)
                            {
                                await this.Storage.SetPeriod(receiver, bizFlag, maxSendLimit.Period).ConfigureAwait(false);
                            }
                            else
                            {
                                await this.Storage.IncreaseSendTimes(receiver, bizFlag).ConfigureAwait(false);
                            }
                        }
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 验证校验码是否正确
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <param name="code">校验码</param>
        /// <param name="maxErrorLimit">最大允许错误次数</param>
        /// <returns></returns>
        public async Task<VerificationResult> VerifyCode(string receiver, string bizFlag, string code, int maxErrorLimit)
        {
            var result = VerificationResult.Expired;
            var vCode = await this.Storage.GetEffectiveCode(receiver, bizFlag).ConfigureAwait(false);
            if (vCode != null && !string.IsNullOrWhiteSpace(vCode.Item1))
            {
                result = VerificationResult.MaxErrorLimit;
                if (vCode.Item2 < maxErrorLimit)
                {
                    result = VerificationResult.Success;
                    if (!string.Equals(vCode.Item1, code, StringComparison.OrdinalIgnoreCase))
                    {
                        result = VerificationResult.VerificationFailed;
                        await this.Storage.IncreaseCodeErrors(receiver, bizFlag).ConfigureAwait(false);
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 获取由数字组成的校验码
        /// </summary>
        /// <param name="maxLength">校验码长度</param>
        /// <returns></returns>
        public static string GetRandomNumber(int maxLength = 6)
        {
            if (maxLength <= 0 || maxLength >= 10)
            {
                throw new ArgumentOutOfRangeException($"{nameof(maxLength)} must between {1} and {9}.");
            }
            var rd = Math.Abs(Guid.NewGuid().GetHashCode());
            var tmpX = (int)Math.Pow(10, maxLength);
            return (rd % tmpX).ToString().PadLeft(maxLength, '0');
        }
    }
}
