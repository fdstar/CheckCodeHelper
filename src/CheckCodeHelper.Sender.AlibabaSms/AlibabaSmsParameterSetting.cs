using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper.Sender.AlibabaSms
{
    /// <summary>
    /// 阿里短信请求参数配置
    /// </summary>
    public class AlibabaSmsParameterSetting
    {
        /// <summary>
        /// 默认的短信签名，如果未设置<see cref="AlibabaSmsRequestParameter.SignName"/>则以此为短信签名
        /// </summary>
        public string DefaultSignName { get; set; }

        /// <summary>
        /// 参数字典 Key为bizFlag
        /// </summary>
        public IDictionary<string, AlibabaSmsRequestParameter> Parameters { get; set; }
        /// <summary>
        /// 阿里短信请求参数 https://next.api.aliyun.com/document/Dysmsapi/2017-05-25/SendSms
        /// </summary>
        public class AlibabaSmsRequestParameter
        {
            /// <summary>
            /// 短信签名名称
            /// </summary>
            public string SignName { get; set; }
            /// <summary>
            /// 短信模板ID
            /// </summary>
            public string TemplateCode { get; set; }
        }
    }
}
