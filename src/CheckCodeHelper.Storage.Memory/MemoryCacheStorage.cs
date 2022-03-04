#if NETSTANDARD2_0_OR_GREATER
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
#else
using System.Runtime.Caching;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper.Storage.Memory
{
    /// <summary>
    /// 校验码信息存储到MemoryCache
    /// </summary>
    public class MemoryCacheStorage : ICodeStorage
    {
        /// <summary>
        /// Code缓存Key值前缀
        /// </summary>
        public string CodeKeyPrefix { get; set; } = "CK";
        /// <summary>
        /// Period缓存Key值前缀
        /// </summary>
        public string PeriodKeyPrefix { get; set; } = "PK";

#if NETSTANDARD2_0_OR_GREATER
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
            this.Cache = cache ?? new MemoryCache(Options.Create(new MemoryCacheOptions()));
        }
#else
        /// <summary>
        /// 基于内存的缓存
        /// </summary>
        public MemoryCache Cache { get; }

        /// <summary>
        /// 缓存逐出优先级别，默认<see cref="CacheItemPriority.NotRemovable"/>
        /// </summary>
        public CacheItemPriority Priority { get; set; } = CacheItemPriority.NotRemovable;

        /// <summary>
        /// 基于<see cref="MemoryCache"/>的构造函数
        /// </summary>
        /// <param name="cache">用于存储的Cache，如果不传则使用<see cref="MemoryCache.Default"/></param>
        public MemoryCacheStorage(MemoryCache cache = null)
        {
            this.Cache = cache ?? MemoryCache.Default;
        }
#endif
        /// <summary>
        /// 释放<see cref="Cache"/>
        /// </summary>
        ~MemoryCacheStorage()
        {
            this.Cache.Dispose();
        }

        /// <summary>
        /// 获取校验码周期内已发送次数，如果周期已到或未发送过任何验证码，则返回0
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="bizFlag"></param>
        /// <returns></returns>
        public Task<int> GetAreadySendTimesAsync(string receiver, string bizFlag)
        {
            var key = this.GetPeriodKey(receiver, bizFlag);
            int times = 0;
            var storage = this.GetFromCache(key);
            if (storage != null)
            {
                times = storage.Number;
            }
#if DEBUG
            Console.WriteLine("Method:{0} Result:{1}", nameof(GetAreadySendTimesAsync), times);
#endif
            return Task.FromResult(times);
        }
        /// <summary>
        /// 获取校验码及已尝试错误次数，如果校验码不存在或已过期，则返回null
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <returns></returns>
        public Task<Tuple<string, int>> GetEffectiveCodeAsync(string receiver, string bizFlag)
        {
            Tuple<string, int> tuple = null;
            var key = this.GetCodeKey(receiver, bizFlag);
            var storage = this.GetFromCache(key);
            if (storage != null)
            {
                tuple = Tuple.Create(storage.Code, storage.Number);
#if DEBUG
                Console.WriteLine("Method:{0} Result:  Code {1} Errors {2} ", nameof(GetEffectiveCodeAsync), storage.Code, storage.Number);
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
        public Task IncreaseCodeErrorsAsync(string receiver, string bizFlag)
        {
            var key = this.GetCodeKey(receiver, bizFlag);
            var storage = this.GetFromCache(key);
            if (storage != null)
            {
                storage.Number += 1;
            }
            return Task.FromResult(0);
        }
        /// <summary>
        /// 校验码周期内发送次数+1，如果周期已到，则不进行任何操作
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <returns></returns>
        public Task IncreaseSendTimesAsync(string receiver, string bizFlag)
        {
            var key = this.GetPeriodKey(receiver, bizFlag);
            var storage = this.GetFromCache(key);
            if (storage != null)
            {
                storage.Number += 1;
            }
            return Task.FromResult(0);
        }
        /// <summary>
        /// 将校验码进行持久化，如果接收方和业务标志组合已经存在，则进行覆盖
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <param name="code">校验码</param>
        /// <param name="effectiveTime">校验码有效时间范围</param>
        /// <returns></returns>
        public Task<bool> SetCodeAsync(string receiver, string bizFlag, string code, TimeSpan effectiveTime)
        {
            var storage = new CodeStorage
            {
                Code = code,
                Number = 0,
                StorageTime = DateTimeOffset.Now
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
        public Task<bool> SetPeriodAsync(string receiver, string bizFlag, TimeSpan? period)
        {
            var storage = new CodeStorage
            {
                Number = 1,
            };
            var key = this.GetPeriodKey(receiver, bizFlag);
            this.SetCache(key, storage, period);
            return Task.FromResult(true);
        }
        /// <summary>
        /// 移除周期限制以及错误次数（适用于登录成功后，错误次数限制重新开始计时的场景）
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <returns>执行结果</returns>
        public Task RemovePeriodAsync(string receiver, string bizFlag)
        {
            var periodKey = this.GetPeriodKey(receiver, bizFlag);
            var codeKey = this.GetCodeKey(receiver, bizFlag);
            this.Cache.Remove(periodKey);
            this.Cache.Remove(codeKey);
            return Task.FromResult(0);
        }
        private void SetCache(string key, CodeStorage storage, TimeSpan? absoluteToNow)
        {
#if NETSTANDARD2_0_OR_GREATER
            var option = new MemoryCacheEntryOptions
            {
                Priority = this.CacheItemPriority,
                AbsoluteExpirationRelativeToNow = absoluteToNow
            };
            this.Cache.Set(key, storage, option);
#else
            var policy = new CacheItemPolicy
            {
                Priority = this.Priority,
            };
            if (absoluteToNow.HasValue)
            {
                policy.AbsoluteExpiration = DateTimeOffset.Now.Add(absoluteToNow.Value);
            }
            this.Cache.Set(key, storage, policy);
#endif
        }
        private CodeStorage GetFromCache(string key)
        {
#if NETSTANDARD2_0_OR_GREATER
            this.Cache.TryGetValue(key, out CodeStorage storage);
            return storage;
#else
            return this.Cache.Get(key) as CodeStorage;
#endif
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
        /// <summary>
        /// 获取最后一次校验码持久化的时间
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="bizFlag"></param>
        /// <returns></returns>
        public Task<DateTimeOffset?> GetLastSetCodeTimeAsync(string receiver, string bizFlag)
        {
            DateTimeOffset? dt = null;
            var key = this.GetCodeKey(receiver, bizFlag);
            var storage = this.GetFromCache(key);
            if (storage != null)
            {
                dt = storage.StorageTime;
            }
            return Task.FromResult(dt);
        }

        [Serializable]
        private sealed class CodeStorage
        {
            public string Code { get; set; }
            public int Number { get; set; }
            public DateTimeOffset StorageTime { get; set; }
        }
    }
}
