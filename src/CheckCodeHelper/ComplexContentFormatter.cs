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
        private readonly ConcurrentDictionary<string, IContentFormatter> _dic = new ConcurrentDictionary<string, IContentFormatter>();
        /// <summary>
        /// 设置指定业务对应的内容模板
        /// </summary>
        /// <param name="bizFlag">业务标志</param>
        /// <param name="senderKey"><see cref="ICodeSender.Key"/></param>
        /// <param name="formatter">内容模板</param>
        public void SetFormatter(string bizFlag, string senderKey, IContentFormatter formatter)
        {
            if (!string.IsNullOrWhiteSpace(bizFlag) && formatter != null)
            {
                this._dic.AddOrUpdate(this.GetKey(bizFlag, senderKey), formatter, (k, v) => formatter);
            }
        }
        private string GetKey(string bizFlag, string senderKey)
        {
            return $"{senderKey}_{bizFlag}";
        }
        /// <summary>
        /// 移除指定业务对应的内容模板，如果没有，则返回null
        /// </summary>
        /// <param name="bizFlag">业务标志</param>
        /// <param name="senderKey"><see cref="ICodeSender.Key"/></param>
        /// <returns></returns>
        public IContentFormatter RemoveFormatter(string bizFlag, string senderKey)
        {
            if (!string.IsNullOrWhiteSpace(bizFlag)
                && this._dic.TryRemove(this.GetKey(bizFlag, senderKey), out IContentFormatter formatter))
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
        /// <param name="senderKey"><see cref="ICodeSender.Key"/></param>
        /// <returns></returns>
        public string GetContent(string receiver, string bizFlag, string code, TimeSpan effectiveTime, string senderKey = null)
        {
            if (string.IsNullOrWhiteSpace(bizFlag))
            {
                throw new ArgumentException(nameof(bizFlag));
            }
            var key = this.GetKey(bizFlag, senderKey);
            this._dic.TryGetValue(key, out IContentFormatter formatter);
            if (formatter == null)
            {
                throw new KeyNotFoundException($"There is no formatter with key '{key}'");
            }
            return formatter.GetContent(receiver, bizFlag, code, effectiveTime, senderKey);
        }
    }
}
