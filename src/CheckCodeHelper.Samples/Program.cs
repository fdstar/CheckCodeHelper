using CheckCodeHelper.Sender.EMail;
using CheckCodeHelper.Sender.Sms;
using CheckCodeHelper.Storage.MemoryCache;
using CheckCodeHelper.Storage.Redis;
using CheckCodeHelper.Storage.RedisCache;
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
            //storage = GetRedisCacheStorage();
            //storage = GetStorageWithRedis();
            storage = GetMemoryCacheStorage();
            //sender = new NoneSender(); //无需发送验证码场景
            //sender = GetSmsSender();
            //sender = GetEMailSender();

            CheckCodeHelperDemo(storage, sender);
            Console.ReadLine();
        }
        private static void CheckCodeHelperDemo(ICodeStorage storage, ICodeSender sender = null)
        {
            if (sender == null)
            {
                sender = new ConsoleSender(GetFormatter(bizFlag)); 
            }

            var helper = new CodeHelper(sender, storage);
            var code = CodeHelper.GetRandomNumber();
            var sendResult = helper.SendCode(receiver, bizFlag, code, effectiveTime, new PeriodLimit
            {
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
                    var vResult = helper.VerifyCode(receiver, bizFlag, vCode, 3).Result;
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
            var sender = new EMailSender(GetFormatter(bizFlag), setting, func)
            {
                TextFormat = MimeKit.Text.TextFormat.Plain//设置发送的邮件内容格式
            };
            return sender;
        }
        private static ICodeSender GetSmsSender()
        {
            //此处以亿美为例
            string host = "shmtn.b2m.cn:80";
            string appid = "11";//填入亿美appid
            string secretKey = "22"; //填入亿美secretKey
            ISms sms = new EmaySms(host, appid, secretKey);
            var sender = new SmsSender(GetFormatter(bizFlag), sms);
            return sender;
        }
        #endregion
        #region IContentFormatter
        private static IContentFormatter GetFormatter(string bizFlag)
        {
            var simpleFormatter = new ContentFormatter(
                    (r, b, c, e) => $"{r}您好，您的忘记密码验证码为{c}，有效期为{(int)e.TotalSeconds}秒.");
            //如果就一个业务场景，也可以直接返回simpleFormatter
            var formatter = new ComplexContentFormatter();
            formatter.SetFormatter(bizFlag, simpleFormatter);
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
