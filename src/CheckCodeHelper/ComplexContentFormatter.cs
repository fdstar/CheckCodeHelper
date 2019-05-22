using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper
{
    using System.Collections.Concurrent;
    /// <summary>
    /// 基于业务标志的多内容模板实现
    /// </summary>
    public class ComplexContentFormatter : IComplexContentFormatter
    {
        private ConcurrentDictionary<string, IContentFormatter> _dic = new ConcurrentDictionary<string, IContentFormatter>();
        /// <summary>
        /// 设置指定业务对应的内容模板
        /// </summary>
        /// <param name="bizFlag">业务标志</param>
        /// <param name="formatter">内容模板</param>
        public void SetFormatter(string bizFlag, IContentFormatter formatter)
        {
            if (!string.IsNullOrWhiteSpace(bizFlag) && formatter != null)
            {
                this._dic.AddOrUpdate(bizFlag, formatter, (k, v) => formatter);
            }
        }
        /// <summary>
        /// 移除指定业务对应的内容模板，如果没有，则返回null
        /// </summary>
        /// <param name="bizFlag">业务标志</param>
        /// <returns></returns>
        public IContentFormatter RemoveFormatter(string bizFlag)
        {
            if (!string.IsNullOrWhiteSpace(bizFlag)
                && this._dic.TryRemove(bizFlag, out IContentFormatter formatter))
            {
                return formatter;
            }
            return null;
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
            if (string.IsNullOrWhiteSpace(bizFlag))
            {
                throw new ArgumentNullException(nameof(bizFlag));
            }
            this._dic.TryGetValue(bizFlag, out IContentFormatter formatter);
            if (formatter == null)
            {
                throw new KeyNotFoundException(nameof(formatter));
            }
            return formatter.GetContent(receiver, bizFlag, code, effectiveTime);
        }
    }
}
