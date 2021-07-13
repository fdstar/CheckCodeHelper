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
        public EmaySms(string host, string appId, string secretKey)
        {
            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(secretKey))
            {
                throw new ArgumentNullException();
            }
            this._appId = appId;
            var key = new byte[16];
            Array.Copy(Encoding.UTF8.GetBytes(secretKey.PadRight(key.Length)), key, key.Length);
            this._secretKey = key;
            this.Client = new RestClient(host);
        }
        /// <summary>
        /// 请求有效期（秒），默认60
        /// </summary>
        public int ValidPeriod { get; set; } = 60;
        /// <summary>
        /// 请求时是否需要Gzip压缩，默认true
        /// </summary>
        public bool UseGzip { get; set; } = true;
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
            var useGZip = this.UseGzip;
            var request = this.GetRestRequest(data, SendSingleSmsUrl, useGZip);
            var response = this.Client.Execute(request);
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
        public async Task<bool> SendMessageAsync(string mobile, string content, string bizId = null, DateTime? sendTime = null)
        {
            var data = this.GetSingleSmsObj(mobile, content, bizId, sendTime);
            var useGZip = this.UseGzip;
            var request = this.GetRestRequest(data, SendSingleSmsUrl, useGZip);
            var response = await this.Client.ExecuteAsync(request).ConfigureAwait(false);
            return this.IsResponseSuccess(response, useGZip);
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
            var useGZip = this.UseGzip;
            var request = this.GetRestRequest(data, SendBatchSmsUrl, useGZip);
            var response = this.Client.Execute(request);
            return this.IsResponseSuccess(response, useGZip);
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
            var useGZip = this.UseGzip;
            var request = this.GetRestRequest(data, SendBatchSmsUrl, useGZip);
            var response = await this.Client.ExecuteAsync(request).ConfigureAwait(false);
            return this.IsResponseSuccess(response, useGZip);
        }
    }
}
