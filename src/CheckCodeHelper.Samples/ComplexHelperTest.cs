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
            services.AddSingletonForEMailSender(configuration.GetSection("EMailSetting"), configuration.GetSection("EMailMimeMessageSetting"));
            services.AddSingletonForAlibabaSms(configuration.GetSection("AlibabaConfig"), configuration.GetSection("AlibabaSmsParameterSetting"));

            //增加自定义ICodeSender例子，且该Sender未设置周期限制
            services.AddSingleton<ICodeSender, ConsoleSender>();

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

            var bizFlag = configuration.GetValue<string>("BizFlag");
            if (bizFlag == "LoginValidError")
            {
                LoginError(complexHelper);
                return;
            }
            else
            {
                CodeValidator(complexHelper);
            }
        }

        private static void CodeValidator(ComplexHelper complexHelper)
        {
            Console.WriteLine("****** 您当前正在进行验证码校验Demo ******");
            var bizFlag = configuration.GetValue<string>("BizFlag");
            var senderKey = configuration.GetValue<string>("CurrentSenderKey");
            var receiver = configuration.GetValue<string>("Receiver");
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
                    if (vResult != VerificationResult.Failed)
                    {
                        break;
                    }
                }
            }
        }

        private static void LoginError(ComplexHelper complexHelper)
        {
            Console.WriteLine("****** 您当前正在进行密码校验错误Demo ******");
            var senderKey = NoneSender.DefaultKey; //密码校验错误无需实际发送内容
            var bizFlag = configuration.GetValue<string>("BizFlag");
            var correct = "1"; //密码正确时，得到的code
            var error = "0"; //密码错误时，得到的code
            var correctPwd = "123456";//这里不管输入的是什么账号，密码假设为同一个密码
            Console.WriteLine("所有账户的正确密码为：" + correctPwd);
            do
            {
                var account = ReadLine("** 请输入您的账号 **", "账号不能为空");
                var pwd = ReadLine("** 请输入您的密码 **", "密码不能为空");
                //检验前进行一次发送，因为周期设置成只能发送一次，所以无需关心发送结果
                var sendResult = complexHelper.SendCodeAsync(senderKey, account, bizFlag, correct).Result;
                Console.WriteLine("周期设置结果:" + sendResult + " 注意周期设置结果不影响校验");

                var code = GetCode(pwd);
                var verifyResult = complexHelper.VerifyCodeAsync(senderKey, account, bizFlag, code, true).Result;//密码校验正确时，重置允许密码连续错误的次数
                Console.WriteLine("您账号 {0} 的密码验证结果 {1}", account, verifyResult);
            }
            while (true);

            string GetCode(string pwd)
            {
                return pwd == correctPwd ? correct : error;
            }

            string ReadLine(string title, string errorNotice)
            {
                Console.WriteLine(title);
                do
                {
                    var input = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(input))
                    {
                        Console.WriteLine(errorNotice);
                    }
                    else
                    {
                        return input;
                    }
                }
                while (true);
            }
        }
    }
}
