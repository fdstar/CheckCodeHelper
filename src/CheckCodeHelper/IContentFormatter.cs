using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper
{
    /// <summary>
    /// 发送校验码内容模板接口
    /// </summary>
    public interface IContentFormatter
    {
        /// <summary>
        /// 将指定参数组织成待发送的文本内容
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <param name="code">校验码</param>
        /// <param name="effectiveTime">校验码有效时间范围</param>
        /// <param name="senderKey"><see cref="ICodeSender.Key"/></param>
        /// <returns></returns>
        string GetContent(string receiver, string bizFlag, string code, TimeSpan effectiveTime, string senderKey = null);
    }
    /// <summary>
    /// 基于业务标志的多内容模板
    /// </summary>
    public interface IComplexContentFormatter : IContentFormatter
    {
        /// <summary>
        /// 设置指定业务对应的内容模板
        /// </summary>
        /// <param name="bizFlag">业务标志</param>
        /// <param name="senderKey"><see cref="ICodeSender.Key"/></param>
        /// <param name="formatter">内容模板</param>
        void SetFormatter(string bizFlag, string senderKey, IContentFormatter formatter);
        /// <summary>
        /// 移除指定业务对应的内容模板，如果没有，则返回null
        /// </summary>
        /// <param name="bizFlag">业务标志</param>
        /// <param name="senderKey"><see cref="ICodeSender.Key"/></param>
        /// <returns></returns>
        IContentFormatter RemoveFormatter(string bizFlag, string senderKey);
    }
}
