# CheckCodeHelper
适用于发送验证码及校验验证码的场景（比如找回密码功能）  
支持周期内限制最大发送次数，支持单次发送后最大校验错误次数  
存储方案默认提供了`Redis`和`MemoryCache`实现，你也可以自己实现`ICodeStorage`来支持其它存储方案。


[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://mit-license.org/)
| Lib | Version | Summary | .Net |
|---|---|---|---|
|CheckCodeHelper|[![NuGet version (CheckCodeHelper)](https://img.shields.io/nuget/v/CheckCodeHelper.svg?style=flat-square)](https://www.nuget.org/packages/CheckCodeHelper/)|主类库|`.NET45`、`.NET Standard 2.0`|
|CheckCodeHelper.Sender.EMail|[![NuGet version (CheckCodeHelper.Sender.EMail)](https://img.shields.io/nuget/v/CheckCodeHelper.Sender.EMail.svg?style=flat-square)](https://www.nuget.org/packages/CheckCodeHelper.Sender.EMail/)|基于EMail的`ICodeSender`实现|`.NET45`、`.NET Standard 2.0`|
|CheckCodeHelper.Sender.Sms|[![NuGet version (CheckCodeHelper.Sender.Sms)](https://img.shields.io/nuget/v/CheckCodeHelper.Sender.Sms.svg?style=flat-square)](https://www.nuget.org/packages/CheckCodeHelper.Sender.Sms/)|基于非模板短信的`ICodeSender`实现，默认提供`emay`短信实现|`.NET452`、`.NET Standard 2.0`|
|CheckCodeHelper.Sender.AlibabaSms|[![NuGet version (CheckCodeHelper.Sender.AlibabaSms)](https://img.shields.io/nuget/v/CheckCodeHelper.Sender.AlibabaSms.svg?style=flat-square)](https://www.nuget.org/packages/CheckCodeHelper.Sender.AlibabaSms/)|基于阿里模板短信的`ICodeSender`实现|`.NET45`、`.NET Standard 2.0`|
|CheckCodeHelper.Storage.Redis|[![NuGet version (CheckCodeHelper.Storage.Redis)](https://img.shields.io/nuget/v/CheckCodeHelper.Storage.Redis.svg?style=flat-square)](https://www.nuget.org/packages/CheckCodeHelper.Storage.Redis/)|基于Redis的`ICodeStorage`实现|`.NET45`、`.NET Standard 2.0`|
|CheckCodeHelper.Storage.Memory|[![NuGet version (CheckCodeHelper.Storage.Memory)](https://img.shields.io/nuget/v/CheckCodeHelper.Storage.Memory.svg?style=flat-square)](https://www.nuget.org/packages/CheckCodeHelper.Storage.Memory/)|基于MemoryCache的`ICodeStorage`实现|`.NET45`、`.NET Standard 2.0`|


## 如何使用
你可以在此处查看使用例子 https://github.com/fdstar/CheckCodeHelper/blob/master/src/CheckCodeHelper.Samples/Program.cs ，其中`PrevDemo()`为`new`显示声明方式实现的Demo，`ComplexHelperTest`为`.Net Core`依赖注入方式实现的Demo

## Release History
**2021-10-18**
- Release v1.0.3 增加阿里模板短信的`ICodeSender`实现；增加`EffectiveTimeDisplayedInContent`以调整验证码有效期在发送内容中的展示方式，`ComplexHelper`已支持该枚举，`ComplexSetting.EffectiveTimeDisplayed`默认设置为`Seconds`，即在所有的发送内容中以秒对应的数字进行展示，设置为`Auto`时如果有效时间为整360秒或以上且可被360整除，则展示为对应的小时数，有效时间为整60秒或以上且可被60整除，则展示为对应的分钟数；`Sender.EMail`移除不必要的`Subject Func`，同步调整`TextFormat`及`Subject`到`EMailMimeMessageSetting`

**2021-07-20**
- Release v1.0.2 增加校验码发送间隔限制

**2021-07-17**
- Release v1.0.1 增加`ComplexHelper`以统一在单个应用中校验码的发送与验证入口，支持按需调用指定的`ICodeSender`

**2021-07-15**
- Release v1.0.0

