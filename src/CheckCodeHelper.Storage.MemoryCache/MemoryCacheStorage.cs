using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper.Storage.MemoryCache
{
    /// <summary>
    /// 校验码信息存储到MemoryCache
    /// </summary>
    public class MemoryCacheStorage : ICodeStorage
    {
        /// <summary>
        /// Code缓存Key值前缀
        /// </summary>
        public string CodeKeyPrefix { get; set; } = "CC";
        /// <summary>
        /// Period缓存Key值前缀
        /// </summary>
        public string PeriodKeyPrefix { get; set; } = "CCT";
        /// <summary>
        /// 基于内存的缓存
        /// </summary>
        public IMemoryCache Cache { get; }
        /// <summary>
        /// 缓存优先级，默认<see cref="CacheItemPriority.High"/>
        /// </summary>
        public CacheItemPriority CacheItemPriority { get; set; } = CacheItemPriority.High;
        /// <summary>
        /// 基于IMemoryCache的构造函数
        /// </summary>
        /// <param name="cache"></param>
        public MemoryCacheStorage(IMemoryCache cache = null)
        {
            this.Cache = cache ?? new Microsoft.Extensions.Caching.Memory.MemoryCache(Options.Create(new MemoryCacheOptions()));
        }
        /// <summary>
        /// 获取校验码周期内已发送次数，如果周期已到或未发送过任何验证码，则返回0
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="bizFlag"></param>
        /// <returns></returns>
        public Task<int> GetAreadySendTimes(string receiver, string bizFlag)
        {
            var key = this.GetPeriodKey(receiver, bizFlag);
            int times = 0;
            this.Cache.TryGetValue(key, out CodeStorage storage);
            if (storage != null)
            {
                times = storage.Number;
            }
#if DEBUG
            Console.WriteLine("Method:{0} Result:{1}", nameof(GetAreadySendTimes), times);
#endif
            return Task.FromResult(times);
        }
        /// <summary>
        /// 获取校验码及已尝试错误次数，如果校验码不存在或已过期，则返回null
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <returns></returns>
        public Task<Tuple<string, int>> GetEffectiveCode(string receiver, string bizFlag)
        {
            Tuple<string, int> tuple = null;
            var key = this.GetCodeKey(receiver, bizFlag);
            this.Cache.TryGetValue(key, out CodeStorage storage);
            if (storage != null)
            {
                tuple = Tuple.Create(storage.Code, storage.Number);
#if DEBUG
                Console.WriteLine("Method:{0} Result:  Code {1} Errors {2} ", nameof(GetEffectiveCode), storage.Code, storage.Number);
#endif
            }
            return Task.FromResult(tuple);
        }
        /// <summary>
        /// 校验码错误次数+1，如果校验码已过期，则不进行任何操作
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <returns></returns>
        public Task IncreaseCodeErrors(string receiver, string bizFlag)
        {
            var key = this.GetCodeKey(receiver, bizFlag);
            this.Cache.TryGetValue(key, out CodeStorage storage);
            if (storage != null)
            {
                storage.Number += 1;
            }
            return Task.CompletedTask;
        }
        /// <summary>
        /// 校验码周期内发送次数+1，如果周期已到，则不进行任何操作
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <returns></returns>
        public Task IncreaseSendTimes(string receiver, string bizFlag)
        {
            var key = this.GetPeriodKey(receiver, bizFlag);
            this.Cache.TryGetValue(key, out CodeStorage storage);
            if (storage != null)
            {
                storage.Number += 1;
            }
            return Task.CompletedTask;
        }
        /// <summary>
        /// 将校验码进行持久化，如果接收方和业务标志组合已经存在，则进行覆盖
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <param name="code">校验码</param>
        /// <param name="effectiveTime">校验码有效时间范围</param>
        /// <returns></returns>
        public Task<bool> SetCode(string receiver, string bizFlag, string code, TimeSpan effectiveTime)
        {
            var storage = new CodeStorage
            {
                Code = code,
                Number = 0,
            };
            var key = this.GetCodeKey(receiver, bizFlag);
            this.SetCache(key, storage, effectiveTime);
            return Task.FromResult(true);
        }
        /// <summary>
        /// 校验码发送次数周期持久化，如果接收方和业务标志组合已经存在，则进行覆盖
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <param name="period">周期时间范围</param>
        /// <returns></returns>
        public Task<bool> SetPeriod(string receiver, string bizFlag, TimeSpan? period)
        {
            var storage = new CodeStorage
            {
                Number = 1,
            };
            var key = this.GetPeriodKey(receiver, bizFlag);
            this.SetCache(key, storage, period);
            return Task.FromResult(true);
        }
        private void SetCache(string key, CodeStorage storage, TimeSpan? absoluteToNow)
        {
            var option = new MemoryCacheEntryOptions
            {
                Priority = this.CacheItemPriority,
                AbsoluteExpirationRelativeToNow = absoluteToNow
            };
            this.Cache.Set(key, storage, option);
        }
        /// <summary>
        /// 组织IMemoryCache键值
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="bizFlag"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        protected virtual string GetKey(string receiver, string bizFlag, string prefix)
        {
            return string.Format("{0}_{1}_{2}", prefix, bizFlag, receiver);
        }
        private string GetPeriodKey(string receiver, string bizFlag)
        {
            return this.GetKey(receiver, bizFlag, this.PeriodKeyPrefix);
        }
        private string GetCodeKey(string receiver, string bizFlag)
        {
            return this.GetKey(receiver, bizFlag, this.CodeKeyPrefix);
        }
        [Serializable]
        private class CodeStorage
        {
            public string Code { get; set; }
            public int Number { get; set; }
        }
    }
}
