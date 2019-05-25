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
        private readonly Func<string, string, string, TimeSpan, string> _func;
        /// <summary>
        /// 通用实现，这样就无需每种业务类型都要实现<see cref="IContentFormatter"/>
        /// </summary>
        /// <param name="func">传递的委托，参数顺序与<see cref="GetContent(string, string, string, TimeSpan)"/>一致</param>
        public ContentFormatter(Func<string, string, string, TimeSpan, string> func)
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
        /// <returns></returns>
        public string GetContent(string receiver, string bizFlag, string code, TimeSpan effectiveTime)
        {
            return this._func.Invoke(receiver, bizFlag, code, effectiveTime);
        }
    }
}
