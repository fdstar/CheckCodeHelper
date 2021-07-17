#if NETSTANDARD2_0_OR_GREATER
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper
{
    /// <summary>
    /// 仅用于Net Core的注册方法
    /// </summary>
    public static class CodeExtensions
    {
        /// <summary>
        /// 注册<see cref="ComplexHelper"/>，并注册其依赖的<see cref="IComplexContentFormatter"/>和用于获取<see cref="ICodeSender"/>的<see cref="Func{T, TResult}"/>
        /// 注意实际支持的<see cref="ICodeSender"/>需要自行注册
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration">仅包含<see cref="ComplexSetting"/>的配置节点</param>
        /// <returns></returns>
        public static IServiceCollection AddSingletonForComplexHelper(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ComplexSetting>(configuration);
            services.AddSingleton<IComplexContentFormatter, ComplexContentFormatter>();//当前仅注册了formater，还需要内部构造
            services.AddSingleton<IContentFormatter>(p => p.GetService<IComplexContentFormatter>());//用ComplexContentFormatter注册
            services.AddSingleton(p =>//校验码发送者
            {
                Func<string, ICodeSender> func = key =>
                {
                    var senders = p.GetServices<ICodeSender>();
                    var sender = senders.FirstOrDefault(_ => _.Key == key);
                    if (sender == null)
                    {
                        throw new KeyNotFoundException($"There is no sender with key '{key}'");
                    }
                    return sender;
                };
                return func;
            });
            services.AddSingleton<ComplexHelper>();
            return services;
        }

        /// <summary>
        /// 注册<see cref="NoneSender"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingletonForNoneSender(this IServiceCollection services)
        {
            services.AddSingleton<ICodeSender, NoneSender>();
            return services;
        }
    }
}
#endif
