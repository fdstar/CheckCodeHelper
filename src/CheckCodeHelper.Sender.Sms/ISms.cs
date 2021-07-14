using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper.Sender.Sms
{
    /// <summary>
    /// 短信发送接口
    /// </summary>
    public interface ISms
    {
        /// <summary>
        /// 如果存在多供应商或多账号时，可用于区分唯一Key值
        /// </summary>
        string Key { get; set; }
        /// <summary>
        /// 发送单条短信
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="content">短信内容</param>
        /// <param name="bizId">业务Id</param>
        /// <param name="sendTime">定时发送时间（是否支持看实际短信供应商）</param>
        /// <returns></returns>
        bool SendMessage(string mobile, string content, string bizId = null, DateTime? sendTime = null);
        /// <summary>
        /// 发送单条短信
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="content">短信内容</param>
        /// <param name="bizId">业务Id</param>
        /// <param name="sendTime">定时发送时间（是否支持看实际短信供应商）</param>
        /// <returns></returns>
        Task<bool> SendMessageAsync(string mobile, string content, string bizId = null, DateTime? sendTime = null);
        /// <summary>
        /// 批量发送短信
        /// </summary>
        /// <param name="content">短信内容</param>
        /// <param name="mobiles">手机号码集合</param>
        /// <param name="bizIds">该值要么为null，要么Count要与mobiles的Count一致，否则会引发异常</param>
        /// <param name="sendTime">定时发送时间（是否支持看实际短信供应商）</param>
        /// <returns></returns>
        bool SendMessageBatch(string content, IList<string> mobiles, IList<string> bizIds = null, DateTime? sendTime = null);
        /// <summary>
        /// 批量发送短信
        /// </summary>
        /// <param name="content">短信内容</param>
        /// <param name="mobiles">手机号码集合</param>
        /// <param name="bizIds">该值要么为null，要么Count要与mobiles的Count一致，否则会引发异常</param>
        /// <param name="sendTime">定时发送时间（是否支持看实际短信供应商）</param>
        /// <returns></returns>
        Task<bool> SendMessageBatchAsync(string content, IList<string> mobiles, IList<string> bizIds = null, DateTime? sendTime = null);
    }
}
