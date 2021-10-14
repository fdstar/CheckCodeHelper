using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper
{
    /// <summary>
    /// <see cref="ComplexHelper"/>的配置信息
    /// </summary>
    public class ComplexSetting
    {
        /// <summary>
        /// 发送内容中验证码有效时间的显示方式，默认以秒显示
        /// </summary>
        public EffectiveTimeDisplayedInContent EffectiveTimeDisplayed { get; set; } = EffectiveTimeDisplayedInContent.Seconds;
        /// <summary>
        /// 用于构造文本的内容模板
        /// </summary>
        public IDictionary<string, string> ContentFormatters { get; set; }
        /// <summary>
        /// 周期次数限制<see cref="PeriodLimit.MaxLimit"/>
        /// </summary>
        public IDictionary<string, int> PeriodMaxLimits { get; set; }
        /// <summary>
        /// 周期时长限制（秒）<see cref="PeriodLimit.Period"/>
        /// </summary>
        public IDictionary<string, int> PeriodLimitSeconds { get; set; }
        /// <summary>
        /// 周期内校验码发送间隔限制（秒）<see cref="PeriodLimit.Interval"/>
        /// </summary>
        public IDictionary<string, int> PeriodLimitIntervalSeconds { get; set; }
        /// <summary>
        /// 验证码有效时间（秒）
        /// </summary>
        public IDictionary<string, int> CodeEffectiveSeconds { get; set; }
        /// <summary>
        /// 最大校验错误次数
        /// </summary>
        public IDictionary<string, int> CodeMaxErrorLimits { get; set; }
    }
}
