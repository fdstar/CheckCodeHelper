using StackExchange.Redis.Extensions.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper.Storage.RedisCache
{
    /// <summary>
    /// 校验码信息存储到Redis
    /// </summary>
    public class RedisCacheStorage : ICodeStorage
    {
        private readonly IRedisCacheClient _client;
        private const string CodeValueHashKey = "Code";
        private const string CodeErrorHashKey = "Error";
        private const string CodeTimeHashKey = "Time";
        private const string PeriodHashKey = "Number";
        /// <summary>
        /// Code缓存Key值前缀
        /// </summary>
        public string CodeKeyPrefix { get; set; } = "CKC";
        /// <summary>
        /// Period缓存Key值前缀
        /// </summary>
        public string PeriodKeyPrefix { get; set; } = "PKC";
        /// <summary>
        /// 缓存写入Redis哪个库
        /// </summary>
        public int DbNumber { get; set; } = 8;
        /// <summary>
        /// 基于RedisCacheClient的构造函数
        /// </summary>
        /// <param name="client"></param>
        public RedisCacheStorage(IRedisCacheClient client)
        {
            this._client = client;
        }
        /// <summary>
        /// 获取校验码周期内已发送次数，如果周期已到或未发送过任何验证码，则返回0
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="bizFlag"></param>
        /// <returns></returns>
        public async Task<int> GetAreadySendTimesAsync(string receiver, string bizFlag)
        {
            var db = this.GetDatabase();
            var key = this.GetPeriodKey(receiver, bizFlag);
            var times = await db.HashGetAsync<int>(key, PeriodHashKey).ConfigureAwait(false);
#if DEBUG
            Console.WriteLine("Method:{0} Result:{1}", nameof(GetAreadySendTimesAsync), times);
#endif
            return times;
        }
        /// <summary>
        /// 获取校验码及已尝试错误次数，如果校验码不存在或已过期，则返回null
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <returns></returns>
        public async Task<Tuple<string, int>> GetEffectiveCodeAsync(string receiver, string bizFlag)
        {
            var db = this.GetDatabase();
            var key = this.GetCodeKey(receiver, bizFlag);
            var dic = await db.HashGetAsync<string>(key, new string[] {
                    CodeValueHashKey,CodeErrorHashKey
                }).ConfigureAwait(false);
            if (dic != null && dic.Count == 2 && dic.Values.All(x => !string.IsNullOrWhiteSpace(x)))
            {
                var code = dic[CodeValueHashKey];
                int.TryParse(dic[CodeErrorHashKey], out int errors);
#if DEBUG
                Console.WriteLine("Method:{0} Result:  Code {1} Errors {2} ", nameof(GetEffectiveCodeAsync), code, errors);
#endif
                return Tuple.Create(code, errors);
            }
            return null;
        }
        /// <summary>
        /// 校验码错误次数+1，如果校验码已过期，则不进行任何操作
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <returns></returns>
        public async Task IncreaseCodeErrorsAsync(string receiver, string bizFlag)
        {
            var db = this.GetDatabase();
            var key = this.GetCodeKey(receiver, bizFlag);
            if (await db.ExistsAsync(key).ConfigureAwait(false))
            {
                await db.HashIncerementByAsync(key, CodeErrorHashKey, 1).ConfigureAwait(false);
            }
        }
        /// <summary>
        /// 校验码周期内发送次数+1，如果周期已到，则不进行任何操作
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <returns></returns>
        public async Task IncreaseSendTimesAsync(string receiver, string bizFlag)
        {
            var db = this.GetDatabase();
            var key = this.GetPeriodKey(receiver, bizFlag);
            if (await db.ExistsAsync(key).ConfigureAwait(false))
            {
                await db.HashIncerementByAsync(key, PeriodHashKey, 1).ConfigureAwait(false);
            }
        }
        /// <summary>
        /// 将校验码进行持久化，如果接收方和业务标志组合已经存在，则进行覆盖
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <param name="code">校验码</param>
        /// <param name="effectiveTime">校验码有效时间范围</param>
        /// <returns></returns>
        public async Task<bool> SetCodeAsync(string receiver, string bizFlag, string code, TimeSpan effectiveTime)
        {
            var db = this.GetDatabase();
            var key = this.GetCodeKey(receiver, bizFlag);
            await db.HashSetAsync(key, CodeValueHashKey, code).ConfigureAwait(false);
            await db.HashSetAsync(key, CodeErrorHashKey, 0).ConfigureAwait(false);
            await db.HashSetAsync(key, CodeTimeHashKey, DateTimeOffset.Now.ToUnixTimeMilliseconds()).ConfigureAwait(false);
            var ret = await db.UpdateExpiryAsync(key, effectiveTime).ConfigureAwait(false);
#if DEBUG
            Console.WriteLine("Method:{0} Result:{1}", nameof(SetCodeAsync), ret);
#endif
            return ret;
        }
        /// <summary>
        /// 校验码发送次数周期持久化，如果接收方和业务标志组合已经存在，则进行覆盖
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <param name="period">周期时间范围</param>
        /// <returns></returns>
        public async Task<bool> SetPeriodAsync(string receiver, string bizFlag, TimeSpan? period)
        {
            var db = this.GetDatabase();
            var key = this.GetPeriodKey(receiver, bizFlag);
            await db.HashSetAsync(key, PeriodHashKey, 1).ConfigureAwait(false);
            var ret = true;
            if (period.HasValue)
            {
                ret = await db.UpdateExpiryAsync(key, period.Value);
            }
#if DEBUG
            Console.WriteLine("Method:{0} Result:{1}", nameof(SetPeriodAsync), ret);
#endif
            return ret;
        }
        /// <summary>
        /// 移除周期限制（适用于登录成功后，错误次数限制重新开始计时的场景）
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <returns>执行结果</returns>
        public async Task RemovePeriodAsync(string receiver, string bizFlag)
        {
            var db = this.GetDatabase();
            var key = this.GetPeriodKey(receiver, bizFlag);
            await db.RemoveAsync(key).ConfigureAwait(false);
        }
        /// <summary>
        /// 组织Redis键值
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="bizFlag"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        protected virtual string GetKey(string receiver, string bizFlag, string prefix)
        {
            return string.Format("{0}:{1}:{2}", prefix, bizFlag, receiver);
        }
        private string GetPeriodKey(string receiver, string bizFlag)
        {
            return this.GetKey(receiver, bizFlag, this.PeriodKeyPrefix);
        }
        private string GetCodeKey(string receiver, string bizFlag)
        {
            return this.GetKey(receiver, bizFlag, this.CodeKeyPrefix);
        }
        private IRedisDatabase GetDatabase()
        {
            return this._client.GetDb(this.DbNumber);
        }
        /// <summary>
        /// 获取最后一次校验码持久化的时间
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="bizFlag"></param>
        /// <returns></returns>
        public async Task<DateTimeOffset?> GetLastSetCodeTimeAsync(string receiver, string bizFlag)
        {
            DateTimeOffset? dt = null;
            var db = this.GetDatabase();
            var key = this.GetCodeKey(receiver, bizFlag);
            var ts = await db.HashGetAsync<long>(key, CodeTimeHashKey).ConfigureAwait(false);
            if (ts > 0)
            {
                dt = DateTimeOffset.FromUnixTimeMilliseconds(ts);
            }
            return dt;
        }
    }
}
