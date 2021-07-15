#if NETSTANDARD2_0_OR_GREATER
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper.Storage.Redis
{
    /// <summary>
    /// 仅用于Net Core的注册方法
    /// </summary>
    public static class RedisExtensions
    {
        /// <summary>
        /// 注册基于Redis的校验码信息存储，此方法默认认为已注册过<see cref="IConnectionMultiplexer"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingletonForRedisStorage(this IServiceCollection services)
        {
            return services.AddSingletonForRedisStorage((IConnectionMultiplexer)null);
        }

        /// <summary>
        /// 注册基于Redis的校验码信息存储
        /// </summary>
        /// <param name="services"></param>
        /// <param name="connectionMultiplexer">用于存储的Redis，注意如果不为null，会将该对象<see cref="ServiceCollectionServiceExtensions.AddSingleton{TService}(IServiceCollection, TService)"/></param>
        /// <returns></returns>
        public static IServiceCollection AddSingletonForRedisStorage(this IServiceCollection services, IConnectionMultiplexer connectionMultiplexer)
        {
            if (connectionMultiplexer != null)
            {
                services.AddSingleton(connectionMultiplexer);
            }
            services.AddSingleton<ICodeStorage, RedisStorage>();
            return services;
        }

        /// <summary>
        /// 注册基于Redis的校验码信息存储
        /// </summary>
        /// <param name="services"></param>
        /// <param name="redisConfiguration">redis配置字符串，通过该方式会默认注册<see cref="IConnectionMultiplexer"/>为Singleton</param>
        /// <returns></returns>
        public static IServiceCollection AddSingletonForRedisStorage(this IServiceCollection services, string redisConfiguration)
        {
            if (string.IsNullOrWhiteSpace(redisConfiguration))
            {
                throw new ArgumentException(nameof(redisConfiguration));
            }
            var connectionMultiplexer = ConnectionMultiplexer.Connect(redisConfiguration);
            services.AddSingletonForRedisStorage(connectionMultiplexer);
            return services;
        }
    }
}
#endif
