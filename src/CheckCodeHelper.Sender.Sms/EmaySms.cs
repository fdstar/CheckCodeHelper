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
#if NETSTANDARD2_0_OR_GREATER
    using Microsoft.Extensions.Options;
#endif
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
        /// 默认Key值
        /// </summary>
        public const string DefaultKey = "Emay";

#if NETSTANDARD2_0_OR_GREATER
        /// <summary>
        /// 亿美短信 http://www.emay.cn/
        /// </summary>
        /// <param name="option"></param>
        public EmaySms(IOptions<EmaySetting> option)
            : this(option.Value)
        {
        }
#endif

        /// <summary>
        /// 亿美短信 http://www.emay.cn/
        /// </summary>
        /// <param name="setting"></param>
#if NET45_OR_GREATER
        public
#else
        private
#endif
         EmaySms(EmaySetting setting)
        {
            if (setting == null || string.IsNullOrWhiteSpace(setting.Host) || string.IsNullOrWhiteSpace(setting.AppId) || string.IsNullOrWhiteSpace(setting.SecretKey))
            {
                throw new ArgumentNullException();
            }
            this._appId = setting.AppId;
            var key = new byte[16];
            Array.Copy(Encoding.UTF8.GetBytes(setting.SecretKey.PadRight(key.Length)), key, key.Length);
            this._secretKey = key;
            this.Client = new RestClient(setting.Host);
        }
        /// <summary>
        /// IRestClient
        /// </summary>
        public IRestClient Client { get; }
        /// <summary>
        /// 请求有效期（秒），默认60
        /// </summary>
        public int ValidPeriod { get; set; } = 60;
        /// <summary>
        /// 请求时是否需要Gzip压缩，默认true
        /// </summary>
        public bool UseGzip { get; set; } = true;
        /// <summary>
        /// 用于区分唯一Key值，默认<see cref="DefaultKey"/>
        /// </summary>
        public string Key { get; set; } = DefaultKey;

        private IRestRequest GetRestRequest(object data, string url,bool useGZip)
        {
            var str = JsonConvert.SerializeObject(data);
            var request = new RestRequest(url, Method.POST);
            request.AddHeader("appId", this._appId);
            var rawData = Encoding.UTF8.GetBytes(str);
            if (useGZip)
            {
                request.AddHeader("gzip", "on");
                rawData = GZipHelper.Compress(rawData);
            }
            var encryptData = AESHelper.Encrypt(rawData, this._secretKey, null, CipherMode.ECB, PaddingMode.PKCS7);
            request.AddParameter("", encryptData, ParameterType.RequestBody);
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
        private bool IsResponseSuccess(IRestResponse response, bool useGZip)
        {
            bool isSuccess = response.Headers.FirstOrDefault(p => p.Name == "result")?.Value.ToString() == "SUCCESS";
#if DEBUG
            var responseStr = this.GetResponseContent(response, useGZip);
#endif
            return isSuccess;
        }
        private string GetResponseContent(IRestResponse response, bool useGZip)
        {
            var data = AESHelper.Decrypt(response.RawBytes, this._secretKey, null, CipherMode.ECB, PaddingMode.PKCS7);
            if (useGZip)
            {
                data = GZipHelper.Decompress(data);
            }
            return Encoding.UTF8.GetString(data);
        }
        /// <summary>
        /// 调用亿美Api
        /// </summary>
        /// <param name="url"></param>
        /// <param name="dataFunc"></param>
        /// <returns></returns>
        protected bool CallApi(string url, Func<object> dataFunc)
        {
            var data = dataFunc();
            var useGZip = this.UseGzip;
            var request = this.GetRestRequest(data, url, useGZip);
            var response = this.Client.Execute(request);
            return this.IsResponseSuccess(response, useGZip);
        }
        /// <summary>
        /// 调用亿美Api
        /// </summary>
        /// <param name="url"></param>
        /// <param name="dataFunc"></param>
        /// <returns></returns>
        protected async Task<bool> CallApiAsync(string url, Func<object> dataFunc)
        {
            var data = dataFunc();
            var useGZip = this.UseGzip;
            var request = this.GetRestRequest(data, url, useGZip);
            var response = await this.Client.ExecuteAsync(request).ConfigureAwait(false);
            return this.IsResponseSuccess(response, useGZip);
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
            return this.CallApi(SendSingleSmsUrl, () => this.GetSingleSmsObj(mobile, content, bizId, sendTime));
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
            return await this.CallApiAsync(SendSingleSmsUrl, () => this.GetSingleSmsObj(mobile, content, bizId, sendTime));
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
            return this.CallApi(SendSingleSmsUrl, () => this.GetBatchSMSObj(content, mobiles, bizIds, sendTime));
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
            return await this.CallApiAsync(SendSingleSmsUrl, () => this.GetBatchSMSObj(content, mobiles, bizIds, sendTime));
        }
    }

    /// <summary>
    /// 亿美短信配置
    /// </summary>
    public class EmaySetting
    {
        /// <summary>
        /// 亿美短信服务Host
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// 亿美短信应用Id
        /// </summary>
        public string AppId { get; set; }
        /// <summary>
        /// 亿美短信应用秘钥
        /// </summary>
        public string SecretKey { get; set; }
    }
}
