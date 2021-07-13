using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper
{
    /// <summary>
    /// 校验码实际发送接口
    /// </summary>
    public interface ICodeSender
    {
        /// <summary>
        /// 发送校验码内容模板
        /// </summary>
        IContentFormatter Formatter { get; }
        /// <summary>
        /// 用于标志当前sender的唯一Key
        /// </summary>
        string Key { get; set; }
        /// <summary>
        /// 判断接收者是否符合发送条件，例如当前发送者只支持邮箱，而接收方为手机号，则返回结果应当为false
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <returns></returns>
        bool IsSupport(string receiver);
        /// <summary>
        /// 发送校验码信息
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <param name="code">校验码</param>
        /// <param name="effectiveTime">校验码有效时间范围</param>
        /// <returns>发送结果</returns>
        Task<bool> SendAsync(string receiver, string bizFlag, string code, TimeSpan effectiveTime);
    }
}
