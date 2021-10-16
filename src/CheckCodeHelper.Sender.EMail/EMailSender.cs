#if NETSTANDARD2_0_OR_GREATER
using Microsoft.Extensions.Options;
#endif
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static CheckCodeHelper.Sender.EMail.EMailMimeMessageSetting;

namespace CheckCodeHelper.Sender.EMail
{
    /// <summary>
    /// 通过邮件发送验证码
    /// </summary>
    public class EMailSender : ICodeSender
    {
        /// <summary>
        /// 默认设置的<see cref="Key"/>
        /// </summary>
        public const string DefaultKey = "EMAIL";
        private readonly EMailMimeMessageSetting mimeMessageSetting;

#if NETSTANDARD2_0_OR_GREATER
        /// <summary>
        /// 通过邮件发送验证码
        /// </summary>
        /// <param name="formatter">验证码内容模板</param>
        /// <param name="helper">邮件发送者</param>
        /// <param name="mimeMessageSetting">邮件主题配置</param>
        public EMailSender(IContentFormatter formatter, EMailHelper helper, IOptions<EMailMimeMessageSetting> mimeMessageSetting)
        {
            this.Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            this.EMailHelper = helper ?? throw new ArgumentNullException(nameof(helper));
            this.mimeMessageSetting = mimeMessageSetting.Value ?? throw new ArgumentNullException(nameof(mimeMessageSetting));
            if (this.mimeMessageSetting.Parameters == null || this.mimeMessageSetting.Parameters.Count == 0)
            {
                throw new ArgumentException(nameof(this.mimeMessageSetting.Parameters));
            }
        }
#else
        /// <summary>
        /// 通过邮件发送验证码
        /// </summary>
        /// <param name="formatter">验证码内容模板</param>
        /// <param name="emailSetting">邮箱配置</param>
        /// <param name="subjectSetting">邮件主题配置</param>
        public EMailSender(IContentFormatter formatter, EMailSetting emailSetting, EMailMimeMessageSetting subjectSetting)
        {
            this.Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            this.mimeMessageSetting = subjectSetting ?? throw new ArgumentNullException(nameof(subjectSetting));
            this.EMailHelper = new EMailHelper(emailSetting ?? throw new ArgumentNullException(nameof(emailSetting)));
            if (this.mimeMessageSetting.Parameters == null || this.mimeMessageSetting.Parameters.Count == 0)
            {
                throw new ArgumentException(nameof(this.mimeMessageSetting.Parameters));
            }
        }
#endif
        /// <summary>
        /// 发送验证码内容模板
        /// </summary>
        public IContentFormatter Formatter { get; }
        /// <summary>
        /// 用于标志当前sender的唯一Key
        /// </summary>
        public string Key { get; set; } = DefaultKey;
        /// <summary>
        /// 邮件发送者
        /// </summary>
        public EMailHelper EMailHelper { get; }
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
            var parameter = this.GetParameter(bizFlag);
            var content = this.Formatter.GetContent(receiver, bizFlag, code, effectiveTime, this.Key);
            await this.EMailHelper.SendEMailAsync(parameter.Subject, content, new List<MailboxAddress> {
                new MailboxAddress(receiver)
            }, this.GetTextFormat(parameter)).ConfigureAwait(false);
            return true;
        }

        private MimeMessageParameter GetParameter(string bizFlag)
        {
            if (!this.mimeMessageSetting.Parameters.ContainsKey(bizFlag))
            {
                throw new KeyNotFoundException($"The parameter for '{bizFlag}' is not found");
            }
            return this.mimeMessageSetting.Parameters[bizFlag];
        }

        private TextFormat GetTextFormat(MimeMessageParameter parameter)
        {
            var textFormat = parameter.TextFormat;
            if (!textFormat.HasValue)
            {
                textFormat = this.mimeMessageSetting.DefaultTextFormat;
            }
            return textFormat.Value;
        }
    }
}
