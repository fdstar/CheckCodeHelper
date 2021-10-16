using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper.Sender.EMail
{
    /// <summary>
    /// 邮件MIME Message配置
    /// </summary>
    public class EMailMimeMessageSetting
    {
        /// <summary>
        /// 默认的邮件内容格式，如果不设置默认为<see cref="TextFormat.Plain"/>
        /// </summary>
        public TextFormat DefaultTextFormat { get; set; }
        /// <summary>
        /// 邮件配置 Key为bizFlag
        /// </summary>
        public IDictionary<string, MimeMessageParameter> Parameters { get; set; }
        /// <summary>
        /// 邮件参数
        /// </summary>
        public class MimeMessageParameter
        {
            /// <summary>
            /// 邮件主题
            /// </summary>
            public string Subject { get; set; }
            /// <summary>
            /// 邮件内容的格式
            /// </summary>
            public TextFormat? TextFormat { get; set; }
        }
    }
}
