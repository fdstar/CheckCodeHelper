using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CheckCodeHelper.Storage.Redis
{
    /// <summary>
    /// 校验码信息存储到Redis
    /// </summary>
    public class RedisStorage : ICodeStorage
    {
        private readonly IConnectionMultiplexer _multiplexer;
        private const string CodeValueHashKey = "Code";
        private const string CodeErrorHashKey = "Error";
        private const string CodeTimeHashKey = "Time";
        private const string PeriodHashKey = "Number";

        private const string SetCodeScript = @"redis.call('HMSET', KEYS[1], 'Code', ARGV[1], 'Error', ARGV[2], 'Time', ARGV[3])
                if ARGV[4] ~= '-1' then
                  return redis.call('PEXPIRE', KEYS[1], ARGV[4])
                end
                return 1";
        private const string SetPeriodScript = @"redis.call('HMSET', KEYS[1], 'Number', ARGV[1])
                if ARGV[2] ~= '-1' then
                  return redis.call('PEXPIRE', KEYS[1], ARGV[2])
                end
                return 1";
        private const string HashIncrementScript = @"local hasKey = redis.call('EXISTS', KEYS[1])
                if hasKey ~= '0' then
                  redis.call('HINCRBY', KEYS[1], KEYS[2], ARGV[1])
                end
                return 1";

        /// <summary>
        /// Code缓存Key值前缀
        /// </summary>
        public string CodeKeyPrefix { get; set; } = "CK";
        /// <summary>
        /// Period缓存Key值前缀
        /// </summary>
        public string PeriodKeyPrefix { get; set; } = "PK";
        /// <summary>
        /// 缓存写入Redis哪个库
        /// </summary>
        public int DbNumber { get; set; } = 8;
        /// <summary>
        /// 基于IConnectionMultiplexer的构造函数
        /// </summary>
        /// <param name="multiplexer"></param>
        public RedisStorage(IConnectionMultiplexer multiplexer)
        {
            this._multiplexer = multiplexer;
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
            var value = await db.HashGetAsync(key, PeriodHashKey).ConfigureAwait(false);
            value.TryParse(out int times);
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
            var values = await db.HashGetAsync(key, new RedisValue[] {
                    CodeValueHashKey,
                    CodeErrorHashKey,
                }).ConfigureAwait(false);
            if (values != null && values.Length == 2 && values.All(x => x.HasValue))
            {
                var code = values[0].ToString();
                values[1].TryParse(out int errors);
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
            await db.ScriptEvaluateAsync(HashIncrementScript, new RedisKey[] { key, CodeErrorHashKey },
                new RedisValue[] { 1 }).ConfigureAwait(false);
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
            await db.ScriptEvaluateAsync(HashIncrementScript, new RedisKey[] { key, PeriodHashKey },
               new RedisValue[] { 1 }).ConfigureAwait(false);
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
#if NETSTANDARD2_0_OR_GREATER
            var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
#else
            var timestamp = DateTimeOffsetHelper.ToUnixTimeMilliseconds(DateTimeOffset.Now);
#endif
            var ms = this.GetMilliseconds(effectiveTime);
            var ret = await db.ScriptEvaluateAsync(SetCodeScript, new RedisKey[] { key },
                new RedisValue[] { code, 0, timestamp, ms }).ConfigureAwait(false);
#if DEBUG
            Console.WriteLine("Method:{0} Result:{1}", nameof(SetCodeAsync), ret);
#endif
            return (int)ret == 1;
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
            var ms = this.GetMilliseconds(period);
            var ret = await db.ScriptEvaluateAsync(SetPeriodScript, new RedisKey[] { key },
                new RedisValue[] { 1 , ms }).ConfigureAwait(false);
#if DEBUG
            Console.WriteLine("Method:{0} Result:{1}", nameof(SetPeriodAsync), ret);
#endif
            return (int)ret == 1;
        }
        /// <summary>
        /// 移除周期限制以及错误次数（适用于登录成功后，错误次数限制重新开始计时的场景）
        /// </summary>
        /// <param name="receiver">接收方</param>
        /// <param name="bizFlag">业务标志</param>
        /// <returns>执行结果</returns>
        public async Task RemovePeriodAsync(string receiver, string bizFlag)
        {
            var db = this.GetDatabase();
            var periodKey = this.GetPeriodKey(receiver, bizFlag);
            var codeKey = this.GetCodeKey(receiver, bizFlag);
            await db.KeyDeleteAsync(new RedisKey[] { periodKey, codeKey }).ConfigureAwait(false);
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
        private IDatabase GetDatabase()
        {
            return this._multiplexer.GetDatabase(this.DbNumber);
        }
        private long GetMilliseconds(TimeSpan? ts)
        {
            var ms = -1L;
            if (ts.HasValue)
            {
                ms = (long)ts.Value.TotalMilliseconds;
            }
            return ms;
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
            var value = await db.HashGetAsync(key, CodeTimeHashKey).ConfigureAwait(false);
            if (value.HasValue && value.TryParse(out long ts))
            {
#if NETSTANDARD2_0_OR_GREATER
                dt = DateTimeOffset.FromUnixTimeMilliseconds(ts);
#else
                dt = DateTimeOffsetHelper.FromUnixTimeMilliseconds(ts);
#endif
            }
            return dt;
        }
    }
}
