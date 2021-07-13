using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper
{
    /// <summary>
    /// 检验码发送接口空实现，该实现适用于无需发送校验码的场景，比如通过图片验证码展示校验码
    /// </summary>
    public class NoneSender : ICodeSender
    {
        /// <summary>
        /// 默认设置的<see cref="Key"/>
        /// </summary>
        public const string DefaultKey = "NONE";
        /// <summary>
        /// 发送校验码内容模板 因为此实现适用场景无需发送验证码，所以此处会返回<see cref="NotImplementedException"/>
        /// </summary>
        public IContentFormatter Formatter => throw new NotImplementedException();
        /// <summary>
        /// 用于标志当前sender的唯一Key
        /// </summary>
        public string Key { get; set; } = DefaultKey;
        /// <summary>
        /// 判断接收者是否符合发送条件，当前实现永远返回true
        /// </summary>
        /// <param name="receiver"></param>
        /// <returns></returns>
        public bool IsSupport(string receiver) => true;
        /// <summary>
        /// 发送校验码信息，当前实现永远返回true
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="bizFlag"></param>
        /// <param name="code"></param>
        /// <param name="effectiveTime"></param>
        /// <returns></returns>
        public Task<bool> SendAsync(string receiver, string bizFlag, string code, TimeSpan effectiveTime)
            => Task.FromResult(true);
    }
}
