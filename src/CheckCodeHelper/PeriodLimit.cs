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
        /// 周期内允许的最大次数
        /// </summary>
        public int MaxLimit { get; set; }
        /// <summary>
        /// 周期时间，如果不设置，则表示无周期，此时<see cref="MaxLimit"/>代表总共只允许发送多少次
        /// </summary>
        public TimeSpan? Period { get; set; }
    }
}
