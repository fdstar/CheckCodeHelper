using CheckCodeHelper.Sender.Sms.Utils;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper.Sender.Sms
{
    /// <summary>
    /// 亿美短信  http://www.emay.cn/
    /// </summary>
    public class EmaySms : ISms
    {
        private readonly string _appId;
        private readonly byte[] _secretKey;
        private const string SendSingleSmsUrl = "/inter/sendSingleSMS";
        private const string SendBatchSmsUrl = "/inter/sendBatchSMS";
        /// <summary>
        /// IRestClient
        /// </summary>
        public IRestClient Client { get; }
        /// <summary>
        /// 亿美短信 http://www.emay.cn/
        /// </summary>
        /// <param name="host">亿美短信服务Host</param>
        /// <param name="appId">亿美短信应用Id</param>
        /// <param name="secretKey">亿美短信应用秘钥</param>
        /// <param name="scheme">传输协议</param>
        public EmaySms(string host, string appId, string secretKey, string scheme = "http")
        {
            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(secretKey))
            {
                throw new ArgumentNullException();
            }
            this._appId = appId;
            var key = new byte[16];
            Array.Copy(Encoding.UTF8.GetBytes(secretKey.PadRight(key.Length)), key, key.Length);
            this._secretKey = key;
            this.Client = new RestClient(string.Format("{0}://{1}", scheme, host));
            this.Client.AddDefaultHeader("appId", this._appId);
        }
        /// <summary>
        /// 请求有效期（秒），默认60
        /// </summary>
        public int ValidPeriod { get; set; } = 60;
        ///// <summary>
        ///// 请求时是否需要Gzip压缩，默认否
        ///// </summary>
        //public bool UseGzip { get; set; }
        private IRestRequest GetRestRequest(object data, string url)
        {
            var str = JsonConvert.SerializeObject(data);
            var encryptData = AESHelper.Encrypt(Encoding.UTF8.GetBytes(str), this._secretKey, null, CipherMode.ECB, PaddingMode.PKCS7);
            var request = new RestRequest(url, Method.POST);
            request.AddParameter(new Parameter
            {
                Type = ParameterType.RequestBody,
                DataFormat = DataFormat.None,
                Value = encryptData,
                Name = ""
            });
            return request;
        }
        private object GetSingleSmsObj(string mobile, string content, string bizId, DateTime? sendTime)
        {
            var data = new
            {
                mobile,
                content,
                timerTime = sendTime.HasValue ? sendTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty,
                customSmsId = bizId,
                requestTime = DateTime.Now.Ticks,
                requestValidPeriod = this.ValidPeriod
            };
            return data;
        }
        private bool IsResponseSuccess(IRestResponse response)
        {
            return response.Headers.FirstOrDefault(p => p.Name == "result")?.Value.ToString() == "SUCCESS";
        }
        private string GetResponseContent(IRestResponse response)
        {
            var data = AESHelper.Decrypt(response.RawBytes, this._secretKey, null, CipherMode.ECB, PaddingMode.PKCS7);
            return Encoding.UTF8.GetString(data);
        }
        /// <summary>
        /// 发送单条短信
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="content">短信内容</param>
        /// <param name="bizId">业务Id</param>
        /// <param name="sendTime">定时发送时间（是否支持看实际短信供应商）</param>
        /// <returns></returns>
        public bool SendMessage(string mobile, string content, string bizId = null, DateTime? sendTime = null)
        {
            var data = this.GetSingleSmsObj(mobile, content, bizId, sendTime);
            var request = this.GetRestRequest(data, SendSingleSmsUrl);
            var response = this.Client.Execute(request);
            return this.IsResponseSuccess(response);
        }
        /// <summary>
        /// 发送单条短信
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="content">短信内容</param>
        /// <param name="bizId">业务Id</param>
        /// <param name="sendTime">定时发送时间（是否支持看实际短信供应商）</param>
        /// <returns></returns>
        public async Task<bool> SendMessageAsync(string mobile, string content, string bizId = null, DateTime? sendTime = null)
        {
            var data = this.GetSingleSmsObj(mobile, content, bizId, sendTime);
            var request = this.GetRestRequest(data, SendSingleSmsUrl);
            var response = await this.Client.ExecuteTaskAsync(request).ConfigureAwait(false);
            return this.IsResponseSuccess(response);
        }
        /// <summary>
        /// 批量发送短信
        /// </summary>
        /// <param name="content">短信内容</param>
        /// <param name="mobiles">手机号码集合</param>
        /// <param name="bizIds">该值要么为null，要么Count要与mobiles的Count一致，否则会引发异常</param>
        /// <param name="sendTime">定时发送时间（是否支持看实际短信供应商）</param>
        /// <returns></returns>
        public bool SendMessageBatch(string content, IList<string> mobiles, IList<string> bizIds = null, DateTime? sendTime = null)
        {
            var data = this.GetBatchSMSObj(content, mobiles, bizIds, sendTime);
            var request = this.GetRestRequest(data, SendBatchSmsUrl);
            var response = this.Client.Execute(request);
            return this.IsResponseSuccess(response);
        }
        private object GetBatchSMSObj(string content, IList<string> mobiles, IList<string> bizIds, DateTime? sendTime)
        {
            if (bizIds != null && bizIds.Count > 0 && mobiles.Count != bizIds.Count)
            {
                throw new ArgumentException($"{nameof(mobiles)}.Count not equals {nameof(bizIds)}.Count");
            }
            if (bizIds != null && bizIds.Count == 0)
            {
                bizIds = null;
            }
            var items = new System.Collections.ArrayList();
            for (var i = 0; i < mobiles.Count; i++)
            {
                items.Add(new
                {
                    mobile = mobiles[i],
                    customSmsId = bizIds?[i]
                });
            }
            var obj = new
            {
                smses = items,
                content,
                timerTime = sendTime.HasValue ? sendTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty,
                requestTime = DateTime.Now.Ticks,
                requestValidPeriod = this.ValidPeriod
            };
            return obj;
        }
        /// <summary>
        /// 批量发送短信
        /// </summary>
        /// <param name="content">短信内容</param>
        /// <param name="mobiles">手机号码集合</param>
        /// <param name="bizIds">该值要么为null，要么Count要与mobiles的Count一致，否则会引发异常</param>
        /// <param name="sendTime">定时发送时间（是否支持看实际短信供应商）</param>
        /// <returns></returns>
        public async Task<bool> SendMessageBatchAsync(string content, IList<string> mobiles, IList<string> bizIds = null, DateTime? sendTime = null)
        {
            var data = this.GetBatchSMSObj(content, mobiles, bizIds, sendTime);
            var request = this.GetRestRequest(data, SendBatchSmsUrl);
            var response = await this.Client.ExecuteTaskAsync(request).ConfigureAwait(false);
            return this.IsResponseSuccess(response);
        }
    }
}
