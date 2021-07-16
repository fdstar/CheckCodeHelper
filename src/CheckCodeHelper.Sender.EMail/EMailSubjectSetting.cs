using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper.Sender.EMail
{
    /// <summary>
    /// 简单的邮件主题配置
    /// </summary>
    public class EMailSubjectSetting
    {
        /// <summary>
        /// 邮件主题 Key为bizFlag，Value为邮件Subject
        /// </summary>
        public IDictionary<string, string> Subjects { get; set; }
    }
}
