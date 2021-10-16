using CheckCodeHelper.Sender.AlibabaSms;
using CheckCodeHelper.Sender.EMail;
using CheckCodeHelper.Sender.Sms;
using CheckCodeHelper.Storage.Memory;
using CheckCodeHelper.Storage.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CheckCodeHelper.Samples
{
    public static class ComplexHelperTest
    {
        public const string SMSKey = SmsSender.DefaultKey;
        public const string EMAILKey = EMailSender.DefaultKey;
        public const string NONEKey = NoneSender.DefaultKey;

        static IServiceCollection services;
        static IConfiguration configuration;
        static ComplexHelperTest()
        {
            var basePath = Path.GetDirectoryName(typeof(ComplexHelperTest).Assembly.Location);
            services = new ServiceCollection();
            configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(basePath, "appsettings.json"), false)
                .Build();

            Init();
        }

        public static void Init()
        {
            services.AddOptions();

            //注册ICodeSender
            services.AddSingletonForNoneSender();
            services.AddSingletonForSmsSenderWithEmay(configuration.GetSection("EmaySetting"));
            services.AddSingletonForEMailSender(configuration.GetSection("EMailSetting"));
            services.AddSingletonForMimeMessage(configuration.GetSection("EMailMimeMessageSetting"));
            services.AddSingletonForAlibabaSms(configuration.GetSection("AlibabaConfig"), configuration.GetSection("AlibabaSmsParameterSetting"));

            //注册ICodeStorage
            services.AddSingletonForRedisStorage(configuration.GetValue<string>("Redis:Configuration"));
            //services.AddSingletonForMemoryCacheStorage();

            //注册ComplexHelper
            services.AddSingletonForComplexHelper(configuration.GetSection("ComplexSetting"));
        }

        public static void Start()
        {
            var serviceProvider = services.BuildServiceProvider();

            //获取Helper，如果默认的InitComplexContentFormatter不符合业务需求，可继承后重写
            //ComplexHelper依赖ICodeSender.Key来获取实际的验证码发送者，如果找不到，会产生异常
            var complexHelper = serviceProvider.GetRequiredService<ComplexHelper>();

            var senderKey = configuration.GetValue<string>("CurrentSenderKey");
            var receiver = configuration.GetValue<string>("Receiver");
            var bizFlag = configuration.GetValue<string>("BizFlag");
            var code = CodeHelper.GetRandomNumber(); //生成随机的验证码

            Action getTimeAction = () =>
            {
                var time = complexHelper.CodeStorage.GetLastSetCodeTimeAsync(receiver, bizFlag).Result;
                if (time.HasValue)
                {
                    Console.WriteLine("上次发送时间：{0:yy-MM-dd HH:mm:ss.fff}", time.Value.LocalDateTime);
                }
                else
                {
                    Console.WriteLine("未能获取到最后一次发送时间");
                }
                var ts = complexHelper.GetSendCDAsync(senderKey, receiver, bizFlag).Result;
                Console.WriteLine("校验码发送剩余CD时间：{0}秒", ts.TotalSeconds);
            };

            getTimeAction();

            var sendResult = complexHelper.SendCodeAsync(senderKey, receiver, bizFlag, code).Result;

            Console.WriteLine("发送结果：{0} 请求时间：{1:yy-MM-dd HH:mm:ss}", sendResult, DateTime.Now);
            if (sendResult == SendResult.Success || sendResult == SendResult.IntervalLimit)
            {
                Console.WriteLine("*****************************");
                while (true)
                {
                    Console.WriteLine("请输入校验码：");
                    var vCode = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(vCode)) continue;
                    getTimeAction();
                    var vResult = complexHelper.VerifyCodeAsync(senderKey, receiver, bizFlag, vCode).Result;
                    Console.WriteLine("{2:yy-MM-dd HH:mm:ss }校验码 {0} 校验结果：{1}", vCode, vResult, DateTime.Now);
                    if (vResult != VerificationResult.VerificationFailed)
                    {
                        break;
                    }
                }
            }
        }
    }
}
