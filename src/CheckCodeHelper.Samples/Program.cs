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
        static void Main(string[] args)
        {
            //CheckCodeHelperWithRedisDemo(GetStorageWithRedisClient());
            CheckCodeHelperWithRedisDemo(GetStorageWithRedis());
            Console.ReadLine();
        }
        static void CheckCodeHelperWithRedisDemo(ICodeStorage storage)
        {
            var bizFlag = "forgetPassword";
            var receiver = "Receiver";
            var effectiveTime = TimeSpan.FromMinutes(1);
            
            var simpleFormatter = new ContentFormatter(
                    (r, b, c, e) => $"{r}您好，您的忘记密码验证码为{c}，有效期为{(int)e.TotalSeconds}秒.");
            var formatter = new ComplexContentFormatter();
            formatter.SetFormatter(bizFlag, simpleFormatter);
            var sender = new ConsoleSender(formatter); //如果就一个业务场景，也可以直接用simpleFormatter
            //var tmp = storage.SetPeriod(receiver, bizFlag, TimeSpan.FromMinutes(20)).Result;
            var helper = new CodeHelper(sender, storage);
            var code = CodeHelper.GetRandomNumber();
            var sendResult = helper.SendCode(receiver, bizFlag, code, effectiveTime, new PeriodLimit
            {
                MaxLimit = 5,
                Period = TimeSpan.FromMinutes(20)
            }).Result;
            Console.WriteLine("发送结果：{0}", sendResult);
            if (sendResult == SendResult.Success)
            {
                Console.WriteLine("*****************************");
                while (true)
                {
                    Console.WriteLine("请输入校验码：");
                    var vCode = Console.ReadLine();
                    var vResult = helper.VerifyCode(receiver, bizFlag, vCode, 3).Result;
                    Console.WriteLine("校验码 {0} 校验结果：{1}", vCode, vResult);
                    if (vResult != VerificationResult.VerificationFailed)
                    {
                        break;
                    }
                }
            }
        }
        private static ICodeStorage GetStorageWithRedisClient()
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
        private static ICodeStorage GetStorageWithRedis()
        {
            var multiplexer = ConnectionMultiplexer.Connect("127.0.0.1:6379");
            var storage = new RedisStorage(multiplexer);
            return storage;
        }
    }
}
