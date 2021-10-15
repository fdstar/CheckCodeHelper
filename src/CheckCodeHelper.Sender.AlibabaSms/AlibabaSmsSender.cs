#if NETSTANDARD2_0_OR_GREATER
using Microsoft.Extensions.Options;
#endif
using AlibabaCloud.OpenApiClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static CheckCodeHelper.Sender.AlibabaSms.AlibabaSmsParameterSetting;
using AlibabaCloud.SDK.Dysmsapi20170525;
using AlibabaCloud.SDK.Dysmsapi20170525.Models;

namespace CheckCodeHelper.Sender.AlibabaSms
{
    /// <summary>
    /// 通过阿里短信发送校验码
    /// </summary>
    public class AlibabaSmsSender : ICodeSender
    {
        /// <summary>
        /// 默认设置的<see cref="Key"/>
        /// </summary>
        public const string DefaultKey = "ALIBABASMS";
        private readonly AlibabaSmsParameterSetting setting;
        private readonly Config config;

        /// <summary>
        /// 通过阿里短信发送校验码
        /// </summary>
        /// <param name="formatter"></param>
        /// <param name="setting"></param>
        /// <param name="config"></param>
        public AlibabaSmsSender(
            IContentFormatter formatter,
#if NETSTANDARD2_0_OR_GREATER
            IOptions<AlibabaSmsParameterSetting> setting,
            IOptions<Config> config
#else
            AlibabaSmsParameterSetting setting,
            Config config
#endif
            )
        {
            this.Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
#if NETSTANDARD2_0_OR_GREATER
            this.setting = setting.Value;
            this.config = config.Value;
#else
            this.setting = setting ?? throw new ArgumentNullException(nameof(setting));
            this.config = config;
#endif
            if (this.setting.Parameters == null || this.setting.Parameters.Count == 0)
            {
                throw new ArgumentException(nameof(this.setting.Parameters));
            }
        }

        /// <summary>
        /// 发送验证码Json模板
        /// </summary>
        public IContentFormatter Formatter { get; }

        /// <summary>
        /// 用于标志当前sender的唯一Key
        /// </summary>
        public string Key { get; set; } = DefaultKey;

        /// <summary>
        /// 判断接收者是否符合发送条件，目前是宽松判断，即正则 ^1\d{10}$
        /// </summary>
        /// <param name="receiver"></param>
        /// <returns></returns>
        public virtual bool IsSupport(string receiver)
        {
            return !string.IsNullOrWhiteSpace(receiver)
                && Regex.IsMatch(receiver, @"^1\d{10}$");
        }

        /// <summary>
        /// 发送校验码信息
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <param name="code">校验码</param>
        /// <param name="effectiveTime">校验码有效时间范围</param>
        /// <returns></returns>
        public async Task<bool> SendAsync(string receiver, string bizFlag, string code, TimeSpan effectiveTime)
        {
            var param = this.GetRequestParameter(bizFlag);
            var request = new SendSmsRequest
            {
                PhoneNumbers = receiver,
                SignName = this.GetSignName(param, bizFlag),
                TemplateCode = param.TemplateCode,
                TemplateParam = this.Formatter.GetContent(receiver, bizFlag, code, effectiveTime, this.Key),
            };
            var client = this.CreateClient();
            var response = await client.SendSmsAsync(request);
            return string.Equals(response.Body.Code, "OK", StringComparison.OrdinalIgnoreCase);
        }

        private Client CreateClient()
        {
            return new Client(this.config);
        }

        private AlibabaSmsRequestParameter GetRequestParameter(string bizFlag)
        {
            if (!this.setting.Parameters.ContainsKey(bizFlag))
            {
                throw new KeyNotFoundException($"The request parameter for '{bizFlag}' is not found");
            }
            return this.setting.Parameters[bizFlag];
        }

        private string GetSignName(AlibabaSmsRequestParameter parameter, string bizFlag)
        {
            var signName = parameter.SignName;
            if (string.IsNullOrWhiteSpace(signName))
            {
                signName = this.setting.DefaultSignName;
                if (string.IsNullOrWhiteSpace(signName))
                {
                    throw new ArgumentException($"The sign name for '{bizFlag}' is not correct");
                }
            }
            return signName;
        }
    }
}
