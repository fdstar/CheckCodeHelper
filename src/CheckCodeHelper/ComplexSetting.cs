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
        /// 用于构造文本的内容模板
        /// </summary>
        public IDictionary<string, string> ContentFormatters { get; set; }
        /// <summary>
        /// 周期次数限制
        /// </summary>
        public IDictionary<string, int> PeriodMaxLimits { get; set; }
        /// <summary>
        /// 周期时长（秒）
        /// </summary>
        public IDictionary<string, int> PeriodLimitSeconds { get; set; }
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
