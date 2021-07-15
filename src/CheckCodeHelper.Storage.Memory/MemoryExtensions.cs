#if NETSTANDARD2_0_OR_GREATER
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper.Storage.Memory
{
    /// <summary>
    /// 仅用于Net Core的注册方法
    /// </summary>
    public static class MemoryExtensions
    {
        /// <summary>
        /// 注册基于内存的校验码信息存储
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction">The <see cref="Action{MemoryCacheOptions}"/> to configure the provided <see cref="MemoryCacheOptions"/></param>
        /// <returns></returns>
        public static IServiceCollection AddSingletonForMemoryCacheStorage(this IServiceCollection services, Action<MemoryCacheOptions> setupAction = null)
        {
            if (setupAction == null)
            {
                services.AddMemoryCache();
            }
            else
            {
                services.AddMemoryCache(setupAction);
            }
            services.AddSingleton<ICodeStorage, MemoryCacheStorage>();
            return services;
        }
    }
}
#endif
