using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper.Sender.EMail
{
    /// <summary>
    /// 发送邮件辅助类
    /// </summary>
    public class EMailHelper
    {
        /// <summary>
        /// 邮箱配置
        /// </summary>
        public EMailSetting Setting { get; }
        /// <summary>
        /// 邮件发送设置
        /// </summary>
        /// <param name="setting"></param>
        public EMailHelper(EMailSetting setting)
        {
            this.Setting = setting ?? throw new ArgumentNullException(nameof(setting));
        }
        /// <summary>
        /// 发送电子邮件，默认发送方为<see cref="EMailSetting.UserAddress"/>
        /// </summary>
        /// <param name="subject">邮件主题</param>
        /// <param name="content">邮件内容主题</param>
        /// <param name="toAddress">接收方信息</param>
        /// <param name="textFormat">内容主题模式，默认TextFormat.Text</param>
        /// <param name="attachments">附件</param>
        /// <param name="ccAddress">抄送方信息</param>
        /// <param name="bccAddress">密送方信息</param>
        /// <param name="dispose">是否自动释放附件所用Stream</param>
        /// <returns></returns>
        public async Task SendEMailAsync(string subject, string content, IEnumerable<MailboxAddress> toAddress, TextFormat textFormat = TextFormat.Text, IEnumerable<AttachmentInfo> attachments = null, IEnumerable<MailboxAddress> ccAddress = null, IEnumerable<MailboxAddress> bccAddress = null, bool dispose = true)
        {
            await SendEMailAsync(subject, content, new MailboxAddress[] { new MailboxAddress(this.Setting.UserName, this.Setting.UserAddress) }, toAddress, textFormat, attachments, ccAddress, bccAddress, dispose).ConfigureAwait(false);
        }

        /// <summary>
        /// 发送电子邮件
        /// </summary>
        /// <param name="subject">邮件主题</param>
        /// <param name="content">邮件内容主题</param>
        /// <param name="fromAddress">发送方信息</param>
        /// <param name="toAddress">接收方信息</param>
        /// <param name="textFormat">内容主题模式，默认TextFormat.Text</param>
        /// <param name="attachments">附件</param>
        /// <param name="ccAddress">抄送方信息</param>
        /// <param name="bccAddress">密送方信息</param>
        /// <param name="dispose">是否自动释放附件所用Stream</param>
        /// <returns></returns>
        public async Task SendEMailAsync(string subject, string content, MailboxAddress fromAddress, IEnumerable<MailboxAddress> toAddress, TextFormat textFormat = TextFormat.Text, IEnumerable<AttachmentInfo> attachments = null, IEnumerable<MailboxAddress> ccAddress = null, IEnumerable<MailboxAddress> bccAddress = null, bool dispose = true)
        {
            await SendEMailAsync(subject, content, new MailboxAddress[] { fromAddress }, toAddress, textFormat, attachments, ccAddress, bccAddress, dispose).ConfigureAwait(false);
        }

        /// <summary>
        /// 发送电子邮件
        /// </summary>
        /// <param name="subject">邮件主题</param>
        /// <param name="content">邮件内容主题</param>
        /// <param name="fromAddress">发送方信息</param>
        /// <param name="toAddress">接收方信息</param>
        /// <param name="textFormat">内容主题模式，默认TextFormat.Text</param>
        /// <param name="attachments">附件</param>
        /// <param name="ccAddress">抄送方信息</param>
        /// <param name="bccAddress">密送方信息</param>
        /// <param name="dispose">是否自动释放附件所用Stream</param>
        /// <returns></returns>
        public async Task SendEMailAsync(string subject, string content, IEnumerable<MailboxAddress> fromAddress, IEnumerable<MailboxAddress> toAddress, TextFormat textFormat = TextFormat.Text, IEnumerable<AttachmentInfo> attachments = null, IEnumerable<MailboxAddress> ccAddress=null, IEnumerable<MailboxAddress> bccAddress = null, bool dispose = true)
        {
            var message = new MimeMessage();
            message.From.AddRange(fromAddress);
            message.To.AddRange(toAddress);
            if (ccAddress != null && ccAddress.Any())
            {
                message.Cc.AddRange(ccAddress);
            }
            if (bccAddress != null && bccAddress.Any())
            {
                message.Bcc.AddRange(bccAddress);
            }
            message.Subject = subject;
            var body = new TextPart(textFormat)
            {
                Text = content
            };
            MimeEntity entity = body;
            if (attachments != null)
            {
                var mult = new Multipart("mixed")
                {
                    body
                };
                foreach (var att in attachments)
                {
                    if (att.Stream != null)
                    {
                        var attachment = string.IsNullOrWhiteSpace(att.ContentType) ? new MimePart() : new MimePart(att.ContentType);
                        attachment.Content = new MimeContent(att.Stream);
                        attachment.ContentDisposition = new ContentDisposition(ContentDisposition.Attachment);
                        attachment.ContentTransferEncoding = att.ContentTransferEncoding;
                        attachment.FileName = ConvertHeaderToBase64(att.FileName, Encoding.UTF8);//解决附件中文名问题
                        mult.Add(attachment);
                    }
                }
                entity = mult;
            }
            message.Body = entity;
            message.Date = DateTime.Now;
            using (var client = new SmtpClient())
            {
                //创建连接
                await this.SmtpClientSetting(client).ConfigureAwait(false);
                await client.SendAsync(message).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
                if (dispose && attachments != null)
                {
                    foreach (var att in attachments)
                    {
                        att.Dispose();
                    }
                }
            }
        }
        /// <summary>
        /// SmtpClient连接配置
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        protected virtual async Task SmtpClientSetting(SmtpClient client)
        {
            await client.ConnectAsync(this.Setting.Host, this.Setting.Port, this.Setting.UseSsl).ConfigureAwait(false);
            await client.AuthenticateAsync(this.Setting.UserAddress, this.Setting.Password).ConfigureAwait(false);
        }
        private string ConvertToBase64(string inputStr, Encoding encoding)
        {
            return Convert.ToBase64String(encoding.GetBytes(inputStr));
        }
        private string ConvertHeaderToBase64(string inputStr, Encoding encoding)
        {//https://www.cnblogs.com/qingspace/p/3732677.html
            var encode = !string.IsNullOrEmpty(inputStr) && inputStr.Any(c => c > 127);
            if (encode)
            {
                return "=?" + encoding.WebName + "?B?" + ConvertToBase64(inputStr, encoding) + "?=";
            }
            return inputStr;
        }
    }
}