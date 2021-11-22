using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper
{
    /// <summary>
    /// 校验码信息存储接口
    /// </summary>
    public interface ICodeStorage
    {
        /// <summary>
        /// 将校验码进行持久化，如果接收方和业务标志组合已经存在，则进行覆盖
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <param name="code">校验码</param>
        /// <param name="effectiveTime">校验码有效时间范围</param>
        /// <returns>执行结果</returns>
        Task<bool> SetCodeAsync(string receiver, string bizFlag, string code, TimeSpan effectiveTime);
        /// <summary>
        /// 校验码错误次数+1，如果校验码已过期，则不进行任何操作
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <returns>执行结果</returns>
        Task IncreaseCodeErrorsAsync(string receiver, string bizFlag);
        /// <summary>
        /// 校验码发送次数周期持久化，如果接收方和业务标志组合已经存在，则进行覆盖
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <param name="period">周期时间范围</param>
        /// <returns>执行结果</returns>
        Task<bool> SetPeriodAsync(string receiver, string bizFlag, TimeSpan? period);
        /// <summary>
        /// 移除周期限制以及错误次数（适用于登录成功后，错误次数限制重新开始计时的场景）
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <returns>执行结果</returns>
        Task RemovePeriodAsync(string receiver, string bizFlag);
        /// <summary>
        /// 校验码周期内发送次数+1，如果周期已到，则不进行任何操作
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <returns>执行结果</returns>
        Task IncreaseSendTimesAsync(string receiver, string bizFlag);
        /// <summary>
        /// 获取校验码及已尝试错误次数，如果校验码不存在或已过期，则返回null
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <returns>获取结果</returns>
        Task<Tuple<string, int>> GetEffectiveCodeAsync(string receiver, string bizFlag);
        /// <summary>
        /// 获取校验码周期内已发送次数，如果周期已到或未发送过任何验证码，则返回0
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <returns>获取结果</returns>
        Task<int> GetAreadySendTimesAsync(string receiver, string bizFlag);
        /// <summary>
        /// 获取最后一次校验码持久化的时间，如果未能获取到则返回null
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <returns>获取结果</returns>
        Task<DateTimeOffset?> GetLastSetCodeTimeAsync(string receiver, string bizFlag);
    }
}
