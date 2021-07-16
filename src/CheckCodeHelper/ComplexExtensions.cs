#if NETSTANDARD2_0_OR_GREATER
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
    public static class ComplexExtensions
    {
        /// <summary>
        /// 注册<see cref="ComplexHelper"/>，并注册其依赖的<see cref="IComplexContentFormatter"/>和用于获取<see cref="ICodeSender"/>的<see cref="Func{T, TResult}"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingletonForComplexHelper(this IServiceCollection services)
        {
            services.AddSingleton<IComplexContentFormatter, ComplexContentFormatter>();//当前仅注册了formater，还需要内部构造
            services.AddSingleton<IContentFormatter>(p => p.GetService<IComplexContentFormatter>());//用ComplexContentFormatter注册
            services.AddSingleton(p =>//校验码发送者
            {
                Func<string, ICodeSender> func = key =>
                {
                    var senders = p.GetServices<ICodeSender>();
                    return senders.First(_ => _.Key == key);
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
