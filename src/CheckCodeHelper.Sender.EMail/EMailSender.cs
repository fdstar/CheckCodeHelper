using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CheckCodeHelper.Sender.EMail
{
    /// <summary>
    /// 通过邮件发送验证码
    /// </summary>
    public class EMailSender : ICodeSender
    {
        /// <summary>
        /// 通过短信发送验证码
        /// </summary>
        /// <param name="formatter">验证码内容模板</param>
        /// <param name="setting">邮箱配置</param>
        /// <param name="subjectFunc">根据业务标志返回对应的邮件主题</param>
        public EMailSender(IContentFormatter formatter, EMailSetting setting, Func<string, string> subjectFunc)
        {
            this.Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            this.SubjectFunc = subjectFunc ?? throw new ArgumentNullException(nameof(subjectFunc));
            this.EMailHelper = new EMailHelper(setting ?? throw new ArgumentNullException(nameof(setting)));
        }
        /// <summary>
        /// 发送验证码内容模板
        /// </summary>
        public IContentFormatter Formatter { get; }
        /// <summary>
        /// 邮件发送者
        /// </summary>
        public EMailHelper EMailHelper { get; }
        /// <summary>
        /// 用于生成邮件主题的委托
        /// </summary>
        public Func<string, string> SubjectFunc { get; }
        /// <summary>
        /// 发送的邮件内容格式
        /// </summary>
        public TextFormat TextFormat { get; set; }
        /// <summary>
        /// 判断接收者是否符合发送条件
        /// </summary>
        /// <param name="receiver"></param>
        /// <returns></returns>
        public virtual bool IsSupport(string receiver)
        {
            return !string.IsNullOrWhiteSpace(receiver)
                && Regex.IsMatch(receiver, @"^[A-Za-z0-9\u4e00-\u9fa5]+@[a-zA-Z0-9_-]+(\.[a-zA-Z0-9_-]+)+$");
        }
        /// <summary>
        /// 发送校验码信息
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <param name="code">校验码</param>
        /// <param name="effectiveTime">校验码有效时间范围</param>
        /// <returns></returns>
        public virtual async Task<bool> SendAsync(string receiver, string bizFlag, string code, TimeSpan effectiveTime)
        {
            var subject = this.SubjectFunc(bizFlag);
            var content = this.Formatter.GetContent(receiver, bizFlag, code, effectiveTime);
            await this.EMailHelper.SendEMailAsync(subject, content, new List<MailboxAddress> {
                new MailboxAddress(receiver)
            }, this.TextFormat).ConfigureAwait(false);
            return true;
        }
    }
}
