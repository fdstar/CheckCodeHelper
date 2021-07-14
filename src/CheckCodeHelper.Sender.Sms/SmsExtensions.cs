#if NETSTANDARD2_0_OR_GREATER
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper.Sender.Sms
{
    /// <summary>
    /// 仅用于Net Core的注册方法
    /// </summary>
    public static class SmsExtensions
    {
        /// <summary>
        /// 注册亿美短信发送相关的服务,注意此方法仅适用于全系统就一种<see cref="ICodeSender"/>实现的情况
        /// 注意此方法不会注册<see cref="SmsSender"/>依赖的<see cref="IContentFormatter"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration">仅包含<see cref="EmaySetting"/>的配置节点</param>
        /// <returns></returns>
        public static IServiceCollection AddSingletonForSmsSenderWithEmay(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingletonForEmay(configuration);
            services.AddSingleton<ICodeSender, SmsSender>();
            return services;
        }

        /// <summary>
        /// 注册亿美发送短信的相关实现，注意此方法不注册<see cref="ICodeSender"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration">仅包含<see cref="EmaySetting"/>的配置节点</param>
        /// <returns></returns>
        public static IServiceCollection AddSingletonForEmay(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmaySetting>(configuration);
            services.AddSingleton<ISms, EmaySms>();
            return services;
        }
    }
}
#endif
