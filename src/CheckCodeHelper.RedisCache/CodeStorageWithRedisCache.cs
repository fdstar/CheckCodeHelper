using StackExchange.Redis.Extensions.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper.RedisCache
{
    /// <summary>
    /// 校验码信息存储到Redis
    /// </summary>
    public class CodeStorageWithRedisCache : ICodeStorage
    {
        private readonly IRedisCacheClient _client;
        private const string CodeValueHashKey = "Code";
        private const string CodeErrorHashKey = "Error";
        private const string PeriodHashKey = "Period";
        /// <summary>
        /// Code缓存Key值前缀
        /// </summary>
        public string CodeKeyPrefix { get; set; } = "CC";
        /// <summary>
        /// Period缓存Key值前缀
        /// </summary>
        public string PeriodKeyPrefix { get; set; } = "CCT";
        /// <summary>
        /// 缓存写入Redis哪个库
        /// </summary>
        public int DbNumber { get; set; } = 8;
        /// <summary>
        /// 基于RedisCacheClient的构造函数
        /// </summary>
        /// <param name="client"></param>
        public CodeStorageWithRedisCache(IRedisCacheClient client)
        {
            this._client = client;
        }
        /// <summary>
        /// 获取校验码周期内已发送次数，如果周期已到或未发送过任何验证码，则返回0
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="bizFlag"></param>
        /// <returns></returns>
        public async Task<int> GetAreadySendTimes(string receiver, string bizFlag)
        {
            var db = this.GetDatabase();
            var key = this.GetPeriodKey(receiver, bizFlag);
            var times = await db.HashGetAsync<int>(key, PeriodHashKey).ConfigureAwait(false);
#if DEBUG
            Console.WriteLine("Method:{0} Result:{1}", nameof(GetAreadySendTimes), times);
#endif
            return times;
        }
        /// <summary>
        /// 获取校验码及已尝试错误次数，如果校验码不存在或已过期，则返回null
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <returns></returns>
        public async Task<Tuple<string, int>> GetEffectiveCode(string receiver, string bizFlag)
        {
            var db = this.GetDatabase();
            var key = this.GetCodeKey(receiver, bizFlag);
            if (await db.ExistsAsync(key).ConfigureAwait(false))
            {
                var code = await db.HashGetAsync<string>(key, CodeValueHashKey).ConfigureAwait(false);
                var errors = await db.HashGetAsync<int>(key, CodeErrorHashKey).ConfigureAwait(false);
#if DEBUG
                Console.WriteLine("Method:{0} Result:  Code {1} Errors {2} ", nameof(GetEffectiveCode), code, errors);
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
        public async Task IncreaseCodeErrors(string receiver, string bizFlag)
        {
            var db = this.GetDatabase();
            var key = this.GetCodeKey(receiver, bizFlag);
            if (await db.ExistsAsync(key).ConfigureAwait(false))
            {
                //var errors = await db.HashGetAsync<int>(key, CodeErrorHashKey).ConfigureAwait(false);
                //await db.HashSetAsync(key, CodeErrorHashKey, errors + 1).ConfigureAwait(false);
                await db.HashIncerementByAsync(key, CodeErrorHashKey, 1).ConfigureAwait(false);
            }
        }
        /// <summary>
        /// 校验码周期内发送次数+1，如果周期已到，则不进行任何操作
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <returns></returns>
        public async Task IncreaseSendTimes(string receiver, string bizFlag)
        {
            var db = this.GetDatabase();
            var key = this.GetPeriodKey(receiver, bizFlag);
            if (await db.ExistsAsync(key).ConfigureAwait(false))
            {
                //var times = await db.HashGetAsync<int>(key, PeriodHashKey).ConfigureAwait(false);
                //await db.HashSetAsync(key, PeriodHashKey, times + 1).ConfigureAwait(false);
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
        public async Task<bool> SetCode(string receiver, string bizFlag, string code, TimeSpan effectiveTime)
        {
            var db = this.GetDatabase();
            var key = this.GetCodeKey(receiver, bizFlag);
            await db.RemoveAsync(key).ConfigureAwait(false);
            var ret = await db.HashSetAsync(key, CodeValueHashKey, code).ConfigureAwait(false)
            && await db.HashSetAsync(key, CodeErrorHashKey, 0).ConfigureAwait(false)
            && await db.UpdateExpiryAsync(key, effectiveTime);
#if DEBUG
            Console.WriteLine("Method:{0} Result:{1}", nameof(SetCode), ret);
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
        public async Task<bool> SetPeriod(string receiver, string bizFlag, TimeSpan? period)
        {
            var db = this.GetDatabase();
            var key = this.GetPeriodKey(receiver, bizFlag);
            await db.RemoveAsync(key).ConfigureAwait(false);
            var ret = await db.HashSetAsync(key, PeriodHashKey, 1).ConfigureAwait(false);
            if (period.HasValue)
            {
                ret = ret && await db.UpdateExpiryAsync(key, period.Value);
            }
#if DEBUG
            Console.WriteLine("Method:{0} Result:{1}", nameof(SetPeriod), ret);
#endif
            return ret;
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
    }
}
