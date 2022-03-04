#if NETSTANDARD2_0_OR_GREATER
using Microsoft.Extensions.Options;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper
{
    /// <summary>
    /// 组合<see cref="ICodeHelper"/>
    /// </summary>
    public class ComplexHelper
    {
        private readonly Func<string, ICodeSender> senderFunc;

        /// <summary>
        /// 配置信息
        /// </summary>
        public ComplexSetting ComplexSetting { get; }
        /// <summary>
        /// 基于业务标志的多内容模板
        /// </summary>
        public IComplexContentFormatter ComplexContentFormatter { get; }
        /// <summary>
        /// 数据存储
        /// </summary>
        public ICodeStorage CodeStorage { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="setting">配置信息</param>
        /// <param name="complexContentFormatter">模板信息</param>
        /// <param name="codeStorage">要使用的存储方式</param>
        /// <param name="senderFunc">用于根据<see cref="ICodeSender.Key"/>获取对应的<see cref="ICodeSender"/></param>
        public ComplexHelper(
#if NETSTANDARD2_0_OR_GREATER
            IOptions<ComplexSetting> setting,
#else
            ComplexSetting setting,
#endif
            IComplexContentFormatter complexContentFormatter,
            ICodeStorage codeStorage,
            Func<string, ICodeSender> senderFunc)
        {
#if NETSTANDARD2_0_OR_GREATER
            this.ComplexSetting = setting.Value;
#else
            this.ComplexSetting = setting;
#endif
            this.ComplexContentFormatter = complexContentFormatter;
            this.CodeStorage = codeStorage;
            this.senderFunc = senderFunc;
            this.InitComplexContentFormatter(complexContentFormatter);
        }
        /// <summary>
        /// 初始化模板信息，<see cref="ComplexSetting.ContentFormatters"/>不为空时默认按<see cref="IContentFormatter.GetContent(string, string, string, TimeSpan, string)"/>参数顺序进行<see cref="string.Format(string, object[])"/>占位，TimeSpan转化为秒进行占位填充
        /// </summary>
        /// <param name="complexContentFormatter"></param>
        protected virtual void InitComplexContentFormatter(IComplexContentFormatter complexContentFormatter)
        {
            var dic = this.ComplexSetting.ContentFormatters;
            if (dic == null || dic.Count == 0)
            {
                return;
            }
            foreach (var kv in dic)
            {
                var content = kv.Value;
                var tuple = this.GetBizFlagAndSenderKey(kv.Key);
                complexContentFormatter.SetFormatter(tuple.Item1, tuple.Item2, new ContentFormatter(
                    (r, b, c, e, s) => string.Format(content, r, b, c, ContentFormatter.GetNumberDisplayed(e, this.ComplexSetting.EffectiveTimeDisplayed), s)
                    ));
            }
        }
        /// <summary>
        /// 获取两者组合后的唯一标志
        /// </summary>
        /// <param name="senderKey"></param>
        /// <param name="bizFlag"></param>
        /// <returns></returns>
        protected virtual string GetUniqueKey(string senderKey, string bizFlag)
        {
            return $"{senderKey}_{bizFlag}";
        }
        /// <summary>
        /// 通过唯一标志获取相应的业务编号和<see cref="ICodeSender.Key"/>
        /// </summary>
        /// <param name="uniqueKey"></param>
        /// <returns></returns>
        protected virtual Tuple<string, string> GetBizFlagAndSenderKey(string uniqueKey)
        {
            var idx = uniqueKey.IndexOf('_');
            if (idx <= 1 || idx == uniqueKey.Length - 1)
            {
                throw new ArgumentException($"The key '{uniqueKey}' formats error");
            }
            var bizFlag = uniqueKey.Substring(idx + 1);
            var senderKey = uniqueKey.Substring(0, idx);
            return Tuple.Create(bizFlag, senderKey);
        }
        /// <summary>
        /// 获取周期限制设置，返回null表示为设置
        /// </summary>
        /// <param name="senderKey"><see cref="ICodeSender.Key"/></param>
        /// <param name="bizFlag">业务标志</param>
        /// <returns></returns>
        public PeriodLimit GetPeriodLimit(string senderKey, string bizFlag)
        {
            PeriodLimit period = null;
            var uniqueKey = this.GetUniqueKey(senderKey, bizFlag);
            var maxLimit = this.ComplexSetting.PeriodMaxLimits;
            if (maxLimit != null && maxLimit.Count > 0 && maxLimit.ContainsKey(uniqueKey))
            {
                InitPeriodLimit();
                period.MaxLimit = maxLimit[uniqueKey];
                if (this.ComplexSetting.PeriodLimitSeconds.ContainsKey(uniqueKey))
                {
                    period.Period = TimeSpan.FromSeconds(this.ComplexSetting.PeriodLimitSeconds[uniqueKey]);
                }
            }
            if (this.ComplexSetting.PeriodLimitIntervalSeconds.ContainsKey(uniqueKey))
            {
                InitPeriodLimit();
                period.Interval = TimeSpan.FromSeconds(this.ComplexSetting.PeriodLimitIntervalSeconds[uniqueKey]);
            }
            void InitPeriodLimit()
            {
                if (period == null)
                {
                    period = new PeriodLimit();
                }
            }
            return period;
        }
        /// <summary>
        /// 获取验证码的有效时间设置
        /// </summary>
        /// <param name="senderKey"><see cref="ICodeSender.Key"/></param>
        /// <param name="bizFlag">业务标志</param>
        /// <returns></returns>
        public TimeSpan GetCodeEffectiveTime(string senderKey, string bizFlag)
        {
            var dic = this.ComplexSetting.CodeEffectiveSeconds;
            if (dic == null || dic.Count == 0)
            {
                throw new ArgumentException(nameof(this.ComplexSetting.CodeEffectiveSeconds));
            }
            var uniqueKey = this.GetUniqueKey(senderKey, bizFlag);
            if (!dic.ContainsKey(uniqueKey))
            {
                throw new KeyNotFoundException($"The effective time for code with key '{uniqueKey}' is not found");
            }
            return TimeSpan.FromSeconds(dic[uniqueKey]);
        }
        /// <summary>
        /// 获取错误次数限制设置
        /// </summary>
        /// <param name="senderKey"><see cref="ICodeSender.Key"/></param>
        /// <param name="bizFlag">业务标志</param>
        /// <returns></returns>
        public int GetCodeErrorLimit(string senderKey, string bizFlag)
        {
            var dic = this.ComplexSetting.CodeMaxErrorLimits;
            if (dic == null || dic.Count == 0)
            {
                throw new ArgumentException(nameof(this.ComplexSetting.CodeMaxErrorLimits));
            }
            var uniqueKey = this.GetUniqueKey(senderKey, bizFlag);
            if (!dic.ContainsKey(uniqueKey))
            {
                throw new KeyNotFoundException($"The error limits for code with key '{uniqueKey}' is not found");
            }
            return dic[uniqueKey];
        }
        /// <summary>
        /// 获取<see cref="ICodeHelper"/>
        /// </summary>
        /// <param name="senderKey"><see cref="ICodeSender.Key"/></param>
        /// <returns></returns>
        public ICodeHelper GetCodeHelper(string senderKey)
        {
            var sender = this.senderFunc(senderKey);
            var codeHelper = new CodeHelper(sender, this.CodeStorage);
            return codeHelper;
        }

        /// <summary>
        /// 使用指定的<see cref="ICodeSender"/>发送校验码
        /// </summary>
        /// <param name="senderKey"><see cref="ICodeSender.Key"/></param>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <param name="code">校验码</param>
        /// <returns></returns>
        public async Task<SendResult> SendCodeAsync(string senderKey, string receiver, string bizFlag, string code)
        {
            var codeHelper = this.GetCodeHelper(senderKey);
            var period = this.GetPeriodLimit(senderKey, bizFlag);
            var effctiveTime = this.GetCodeEffectiveTime(senderKey, bizFlag);
            return await codeHelper.SendCodeAsync(receiver, bizFlag, code, effctiveTime, period);
        }

        /// <summary>
        /// 使用指定的<see cref="ICodeSender"/>验证校验码是否正确
        /// </summary>
        /// <param name="senderKey"><see cref="ICodeSender.Key"/></param>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <param name="code">校验码</param>
        /// <param name="resetWhileRight">当验证通过时，是否重置周期次数限制，默认false</param>
        /// <returns></returns>
        public async Task<VerificationResult> VerifyCodeAsync(string senderKey, string receiver, string bizFlag, string code, bool resetWhileRight = false)
        {
            var codeHelper = this.GetCodeHelper(senderKey);
            var errorLimit = this.GetCodeErrorLimit(senderKey, bizFlag);
            return await codeHelper.VerifyCodeAsync(receiver, bizFlag, code, errorLimit, resetWhileRight);
        }

        /// <summary>
        /// 获取校验码发送的CD时间，如果无CD时间，则返回<see cref="TimeSpan.Zero"/>
        /// </summary>
        /// <param name="senderKey"><see cref="ICodeSender.Key"/></param>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <returns></returns>
        public async Task<TimeSpan> GetSendCDAsync(string senderKey, string receiver, string bizFlag)
        {
            var codeHelper = this.GetCodeHelper(senderKey);
            var period = this.GetPeriodLimit(senderKey, bizFlag);
            return await codeHelper.GetSendCDAsync(receiver, bizFlag, period);
        }
    }
}
