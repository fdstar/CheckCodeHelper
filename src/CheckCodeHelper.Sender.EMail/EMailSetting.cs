using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper.Sender.EMail
{
    /// <summary>
    /// 邮箱配置
    /// </summary>
    public class EMailSetting
    {
        /// <summary>
        /// 邮件服务器Host
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// 邮件服务器Port
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// 邮件服务器是否是ssl
        /// </summary>
        public bool UseSsl { get; set; }
        /// <summary>
        /// 发送邮件的账号友善名称
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 发送邮件的账号地址
        /// </summary>
        public string UserAddress { get; set; }
        /// <summary>
        /// 发现邮件所需的账号密码
        /// </summary>
        public string Password { get; set; }
    }
}
