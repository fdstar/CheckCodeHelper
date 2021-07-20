using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper
{
    /// <summary>
    /// 校验码发送周期设置
    /// </summary>
    public class PeriodLimit
    {
        /// <summary>
        /// 周期内允许的最大次数，0表示无限制
        /// </summary>
        public int MaxLimit { get; set; }
        /// <summary>
        /// 周期时间，如果不设置，则表示无周期，此时<see cref="MaxLimit"/>代表总共只允许发送多少次
        /// </summary>
        public TimeSpan? Period { get; set; }
        /// <summary>
        /// 验证码发送间隔，如果不设置，表示可以无冷却重新发送验证码，注意该时间最大只能为验证码的有效时间，超出部分无效且容易造成提示错误
        /// 注意间隔不受周期时间影响，例如发送间隔是60秒，周期是12H，在单个周期的最后一秒发送验证码，在下个周期内，还是需要60-1秒之后才可以发送
        /// </summary>
        public TimeSpan? Interval { get; set; }
    }
}
