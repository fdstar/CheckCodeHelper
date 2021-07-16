﻿#if NETSTANDARD2_0_OR_GREATER
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper.Sender.EMail
{
    /// <summary>
    /// 仅用于Net Core的注册方法
    /// </summary>
    public static class EMailExtensions
    {
        /// <summary>
        /// 注册邮件发送相关的服务,注意此方法仅适用于全系统就一种<see cref="ICodeSender"/>实现的情况
        /// 注意此方法不会注册<see cref="EMailSender"/>依赖的<see cref="IContentFormatter"/>以及<see cref="Func{T, TResult}"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration">仅包含<see cref="EMailSetting"/>的配置节点</param>
        /// <returns></returns>
        public static IServiceCollection AddSingletonForEMailSender(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingletonForEMailHelper(configuration);
            services.AddSingleton<ICodeSender, EMailSender>();
            return services;
        }

        /// <summary>
        /// 注册邮件发送辅助类的相关实现，注意此方法不注册<see cref="ICodeSender"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration">仅包含<see cref="EMailSetting"/>的配置节点</param>
        /// <returns></returns>
        public static IServiceCollection AddSingletonForEMailHelper(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EMailSetting>(configuration);
            services.AddSingleton<EMailHelper>();
            return services;
        }

        /// <summary>
        /// 注册简单的邮件主题<see cref="Func{T, TResult}"/>，该委托简单的返回业务标志对应的邮件主题，注意业务标志区分大小写
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration">仅包含<see cref="EMailSubjectSetting"/>的配置节点</param>
        /// <returns></returns>
        public static IServiceCollection AddSingletonForSubject(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EMailSubjectSetting>(configuration);
            services.AddSingleton(p => //邮件主题
            {
                var setting = p.GetRequiredService<IOptions<EMailSubjectSetting>>().Value;
                var emailSubjects = setting.Subjects;
                if (emailSubjects == null || emailSubjects.Count == 0)
                {
                    throw new ArgumentException(nameof(setting.Subjects));
                }
                Func<string, string> func = key =>
                {
                    if (!emailSubjects.ContainsKey(key))
                    {
                        throw new KeyNotFoundException($"The subject for '{key}' is not found");
                    }
                    return emailSubjects[key];
                };
                return func;
            });
            return services;
        }
    }
}
#endif
