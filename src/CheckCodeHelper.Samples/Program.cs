using CheckCodeHelper.Sender.EMail;
using CheckCodeHelper.Sender.Sms;
using CheckCodeHelper.Storage.Memory;
using CheckCodeHelper.Storage.Redis;
using CheckCodeHelper.Storage.RedisCache;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Newtonsoft;
using StackExchange.Redis.Extensions.Protobuf;
using System;

namespace CheckCodeHelper.Samples
{
    class Program
    {
        static readonly string bizFlag="forgetPassword";
        static readonly string receiver = "test";//根据不同的sender，输入不同的接收验证码账号
        static readonly TimeSpan effectiveTime = TimeSpan.FromMinutes(1);
        static void Main(string[] args)
        {
            ICodeSender sender = null;
            ICodeStorage storage;

            //storage = GetRedisCacheStorage(); //基于StackExchange.Redis.Extensions.Core
            //storage = GetRedisStorage(); //仅基于StackExchange.Redis
            storage = GetMemoryCacheStorage(); //基于MemoryCache

            //sender = new NoneSender(); //无需发送验证码场景
            //sender = GetSmsSender(); //通过短信发送验证码
            //sender = GetEMailSender(); //通过邮件发送验证码

            CheckCodeHelperDemo(storage, sender);
            Console.ReadLine();
        }
        private static void CheckCodeHelperDemo(ICodeStorage storage, ICodeSender sender = null)
        {
            if (sender == null)
            {
                var senderKey = "CONSOLE";
                sender = new ConsoleSender(GetFormatter(bizFlag, senderKey))
                {
                    Key = senderKey,
                };
            }

            var helper = new CodeHelper(sender, storage);
            var code = CodeHelper.GetRandomNumber(); //生成随机的验证码

            Action getTimeAction = () =>
            {
                //ICodeStorage.GetLastSetCodeTime用于获取最后一次发送校验码时间
                //用于比如手机验证码发送后，用户刷新页面时，页面上用于按钮倒计时计数的计算
                var time = storage.GetLastSetCodeTimeAsync(receiver, bizFlag).Result;
                if (time.HasValue)
                {
                    Console.WriteLine("上次发送时间：{0:yy-MM-dd HH:mm:ss.fff}", time.Value);
                }
                else
                {
                    Console.WriteLine("未能获取到最后一次发送时间");
                }
            };
            getTimeAction();

            var sendResult = helper.SendCodeAsync(receiver, bizFlag, code, effectiveTime, new PeriodLimit
            {
                //设置周期为20分钟，然后在此段时间内最多允许发送验证码5次
                MaxLimit = 5,
                Period = TimeSpan.FromMinutes(20)
            }).Result;
            
            Console.WriteLine("发送结果：{0} 发送时间：{1:yy-MM-dd HH:mm:ss}", sendResult, DateTime.Now);
            if (sendResult == SendResult.Success)
            {
                Console.WriteLine("*****************************");
                while (true)
                {
                    Console.WriteLine("请输入校验码：");
                    var vCode = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(vCode)) continue;
                    getTimeAction();
                    var vResult = helper.VerifyCodeAsync(receiver, bizFlag, vCode, 3).Result;
                    Console.WriteLine("{2:yy-MM-dd HH:mm:ss }校验码 {0} 校验结果：{1}", vCode, vResult, DateTime.Now);
                    if (vResult != VerificationResult.VerificationFailed)
                    {
                        break;
                    }
                }
            }
        }
        #region ICodeSender
        private static ICodeSender GetEMailSender()
        {
            var setting = new EMailSetting()//设置您的smtp信息
            {
                Host = "smtp.exmail.qq.com",
                Port = 465,
                UseSsl = true,
                UserName = "测试",
                Password = "",//填入发送邮件的邮箱密码
                UserAddress = "",//填入发送邮件的邮箱地址
            };
            string func(string b) => "找回密码验证码测试邮件";
            var helper = new EMailHelper(Options.Create(setting));
            var sender = new EMailSender(GetFormatter(bizFlag, EMailSender.DefaultKey), helper, func)
            {
                TextFormat = MimeKit.Text.TextFormat.Plain//设置发送的邮件内容格式
            };
            return sender;
        }
        private static ICodeSender GetSmsSender()
        {
            //此处以亿美为例
            string host = "http://shmtn.b2m.cn:80";
            string appid = "11";//填入亿美appid
            string secretKey = "22"; //填入亿美secretKey
            ISms sms = new EmaySms(Options.Create(new EmaySetting
            {
                Host = host,
                AppId = appid,
                SecretKey = secretKey
            }));
            var sender = new SmsSender(GetFormatter(bizFlag, SmsSender.DefaultKey), sms);
            return sender;
        }
        #endregion
        #region IContentFormatter
        private static IContentFormatter GetFormatter(string bizFlag,string senderKey)
        {
            var simpleFormatter = new ContentFormatter(
                    (r, b, c, e, s) => $"{r}您好，您的忘记密码验证码为{c}，有效期为{(int)e.TotalSeconds}秒.");
            //如果就一个业务场景，也可以直接返回simpleFormatter
            var formatter = new ComplexContentFormatter();
            formatter.SetFormatter(bizFlag, senderKey, simpleFormatter);
            return formatter;
        }
        #endregion
        #region ICodeStorage
        private static ICodeStorage GetRedisCacheStorage()
        {
            var redisConfig = new RedisConfiguration
            {
                Hosts = new RedisHost[] {
                    new RedisHost{
                         Host="127.0.0.1",
                         Port=6379
                    }
                }
            };
            var redisManager = new RedisCacheConnectionPoolManager(redisConfig);
            var redisClient = new RedisCacheClient(redisManager,
                new NewtonsoftSerializer(), redisConfig);//new ProtobufSerializer();
            var storage = new RedisCacheStorage(redisClient);
            return storage;
        }
        private static ICodeStorage GetRedisStorage()
        {
            var multiplexer = ConnectionMultiplexer.Connect("127.0.0.1:6379");
            var storage = new RedisStorage(multiplexer);
            return storage;
        }
        private static ICodeStorage GetMemoryCacheStorage()
        {
            return new MemoryCacheStorage();
        }
        #endregion
    }
}
