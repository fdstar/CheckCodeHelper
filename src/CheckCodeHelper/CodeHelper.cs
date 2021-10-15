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
        /// <param name="periodLimit">周期内允许的发送配置，为null则表示无限制</param>
        public async Task<SendResult> SendCodeAsync(string receiver, string bizFlag, string code, TimeSpan effectiveTime, PeriodLimit periodLimit)
        {
            var result = SendResult.NotSupprot;
            if (this.Sender.IsSupport(receiver))
            {
                result = SendResult.MaxSendLimit;
                bool canSend = periodLimit == null || periodLimit.MaxLimit <= 0;
                int sendCount = 0;
                if (!canSend)
                {
                    //校验最大次数
                    sendCount = await this.Storage.GetAreadySendTimesAsync(receiver, bizFlag).ConfigureAwait(false);
                    canSend = sendCount < periodLimit.MaxLimit;
                }
                if (canSend)
                {
                    //校验发送间隔
                    result = SendResult.IntervalLimit;
                    canSend = TimeSpan.Zero == await this.GetSendCDAsync(receiver, bizFlag, periodLimit).ConfigureAwait(false);
                }
                if (canSend)
                {
                    //校验发送结果
                    result = SendResult.FailInSend;
                    if (await this.Sender.SendAsync(receiver, bizFlag, code, effectiveTime).ConfigureAwait(false)
                        && await this.Storage.SetCodeAsync(receiver, bizFlag, code, effectiveTime).ConfigureAwait(false))
                    {
                        result = SendResult.Success;
                        if (periodLimit != null)
                        {
                            if (sendCount == 0)
                            {
                                await this.Storage.SetPeriodAsync(receiver, bizFlag, periodLimit.Period).ConfigureAwait(false);
                            }
                            else
                            {
                                await this.Storage.IncreaseSendTimesAsync(receiver, bizFlag).ConfigureAwait(false);
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
        /// <param name="resetWhileRight">当验证通过时，是否重置周期次数限制，默认false</param>
        /// <returns>验证结果</returns>
        public async Task<VerificationResult> VerifyCodeAsync(string receiver, string bizFlag, string code, int maxErrorLimit, bool resetWhileRight = false)
        {
            var result = VerificationResult.Expired;
            var vCode = await this.Storage.GetEffectiveCodeAsync(receiver, bizFlag).ConfigureAwait(false);
            if (vCode != null && !string.IsNullOrWhiteSpace(vCode.Item1))
            {
                result = VerificationResult.MaxErrorLimit;
                if (vCode.Item2 < maxErrorLimit)
                {
                    result = VerificationResult.Success;
                    if (!string.Equals(vCode.Item1, code, StringComparison.OrdinalIgnoreCase))
                    {
                        result = VerificationResult.VerificationFailed;
                        await this.Storage.IncreaseCodeErrorsAsync(receiver, bizFlag).ConfigureAwait(false);
                    }
                    else if(resetWhileRight)
                    {
                        await this.Storage.RemovePeriodAsync(receiver, bizFlag);
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 获取校验码发送的CD时间，如果无CD时间，则返回<see cref="TimeSpan.Zero"/>
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <param name="periodLimit">周期内允许的发送配置，为null则表示无限制</param>
        /// <returns></returns>
        public async Task<TimeSpan> GetSendCDAsync(string receiver, string bizFlag, PeriodLimit periodLimit)
        {
            if (periodLimit != null && periodLimit.Interval > TimeSpan.Zero)
            {
                var lastSendTime = await this.Storage.GetLastSetCodeTimeAsync(receiver, bizFlag).ConfigureAwait(false);
                if (lastSendTime.HasValue)
                {
                    var ts = lastSendTime.Value.Add(periodLimit.Interval.Value) - DateTimeOffset.Now;
                    if (ts > TimeSpan.Zero)
                    {
                        return ts;
                    }
                }
            }
            return TimeSpan.Zero;
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
                throw new ArgumentOutOfRangeException($"{nameof(maxLength)} must between 1 and 9.");
            }
            var rd = Math.Abs(Guid.NewGuid().GetHashCode());
            var tmpX = (int)Math.Pow(10, maxLength);
            return (rd % tmpX).ToString().PadLeft(maxLength, '0');
        }
    }
}
