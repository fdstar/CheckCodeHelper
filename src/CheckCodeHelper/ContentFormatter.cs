using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper
{
    /// <summary>
    /// 通用的内容模板
    /// </summary>
    public class ContentFormatter : IContentFormatter
    {
        private readonly Func<string, string, string, TimeSpan, string, string> _func;
        /// <summary>
        /// 通用实现，这样就无需每种业务类型都要实现<see cref="IContentFormatter"/>
        /// </summary>
        /// <param name="func">传递的委托，参数顺序与<see cref="GetContent(string, string, string, TimeSpan,string)"/>一致</param>
        public ContentFormatter(Func<string, string, string, TimeSpan, string, string> func)
        {
            this._func = func ?? throw new ArgumentNullException(nameof(func));
        }
        /// <summary>
        /// 将指定参数组织成待发送的文本内容
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <param name="code">校验码</param>
        /// <param name="effectiveTime">校验码有效时间范围</param>
        /// <param name="senderKey"><see cref="ICodeSender.Key"/></param>
        /// <returns></returns>
        public string GetContent(string receiver, string bizFlag, string code, TimeSpan effectiveTime, string senderKey = null)
        {
            return this._func.Invoke(receiver, bizFlag, code, effectiveTime, senderKey);
        }

        /// <summary>
        /// 获取用于显示的时间数字
        /// </summary>
        /// <param name="effectiveTime"></param>
        /// <param name="displayed"></param>
        /// <returns></returns>
        public static double GetNumberDisplayed(TimeSpan effectiveTime, EffectiveTimeDisplayedInContent displayed)
        {
            switch (displayed)
            {
                case EffectiveTimeDisplayedInContent.Seconds:
                    return (int)effectiveTime.TotalSeconds;
                case EffectiveTimeDisplayedInContent.Minutes:
                    return effectiveTime.TotalMinutes;
                case EffectiveTimeDisplayedInContent.Hours:
                    return effectiveTime.TotalHours;
                default:
                    int seconds = (int)effectiveTime.TotalSeconds;
                    const int secondsPerMinute = 60;
                    const int secondsPerHour = 360;
                    if (seconds >= secondsPerHour && seconds % secondsPerHour == 0)
                    {
                        return seconds / secondsPerHour;
                    }
                    else if (seconds >= secondsPerMinute && seconds % secondsPerMinute == 0)
                    {
                        return seconds / secondsPerMinute;
                    }
                    return seconds;
            }
        }
    }

    /// <summary>
    /// 发送内容中验证码有效时间的显示方式
    /// </summary>
    public enum EffectiveTimeDisplayedInContent
    {
        /// <summary>
        /// 自动判断
        /// 当有效时间大于等于60秒，且可被60整除时，以分钟显示
        /// 当有效时间大于等于360秒，且可被360整除时，以小时显示
        /// 其它情况，以整数秒显示
        /// </summary>
        Auto = 0,
        /// <summary>
        /// 强制以整数秒显示，如果所有验证码有效时间都较短时，可以强制指定该方式
        /// </summary>
        Seconds = 1,
        /// <summary>
        /// 强制以分钟显示，注意此时会以<see cref="TimeSpan.TotalMinutes"/>进行显示
        /// </summary>
        Minutes = 2,
        /// <summary>
        /// 强制以小时显示，注意此时会以<see cref="TimeSpan.TotalHours"/>进行显示
        /// </summary>
        Hours = 3,
    }
}
