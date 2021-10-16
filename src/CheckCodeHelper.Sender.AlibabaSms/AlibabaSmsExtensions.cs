#if NETSTANDARD2_0_OR_GREATER
using AlibabaCloud.OpenApiClient.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper.Sender.AlibabaSms
{
    /// <summary>
    /// 仅用于Net Core的注册方法
    /// </summary>
    public static class AlibabaSmsExtensions
    {
        /// <summary>
        /// 注册阿里短信发送相关的服务,注意此方法仅适用于全系统就一种<see cref="AlibabaSmsSender"/>的情况
        /// 注意此方法不会注册<see cref="AlibabaSmsSender"/>依赖的<see cref="IContentFormatter"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config">仅包含<see cref="AlibabaCloud.OpenApiClient.Models.Config"/>的配置节点</param>
        /// <param name="setting">仅包含<see cref="AlibabaSmsParameterSetting"/>的配置节点</param>
        /// <returns></returns>
        public static IServiceCollection AddSingletonForAlibabaSms(this IServiceCollection services, IConfiguration config, IConfiguration setting)
        {
            services.Configure<Config>(config);
            services.Configure<AlibabaSmsParameterSetting>(setting);
            services.AddSingleton<ICodeSender, AlibabaSmsSender>();
            return services;
        }
    }
}
#endif
