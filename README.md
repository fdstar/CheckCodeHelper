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
你可以在此处查看使用例子 https://github.com/fdstar/CheckCodeHelper/blob/master/src/CheckCodeHelper.Samples 
- `Program.PrevDemo()`为`new`显示声明方式实现的Demo
- `ComplexHelperTest`为`.Net Core`依赖注入方式实现的Demo
- `ConsoleSender`为自定义`ICodeSender`及`ICodeSenderSupportAsync`的例子

## Release History
**Unreleased**
|CheckCodeHelper.Storage.Redis v1.0.2|
|:--|
|`ICodeStorage.RemovePeriodAsync`同时移除周期限制及错误次数记录|
|**CheckCodeHelper.Storage.Memory v1.0.1**|
|`ICodeStorage.RemovePeriodAsync`同时移除周期限制及错误次数记录|

**2021-11-03 Release** 
|CheckCodeHelper v1.0.4|
|:--|
|增加`ICodeSenderSupportAsync`以支持`ICodeSender.IsSupport`异步场景，如果`ICodeSender`同时实现了`ICodeSenderSupportAsync`，则`CodeHelper`会通过`ICodeSenderSupportAsync.IsSupportAsync`判断`SendResult.NotSupport`|
|修正`ComplexHelper`获取`PeriodLimit`时，如果未配置`ComplexSetting.PeriodMaxLimits`会导致`ComplexSetting.PeriodLimitIntervalSeconds`无效的问题|

**2021-10-20 Release** 
|<div style="width:100px">CheckCodeHelper.Storage.Redis v1.0.1</div>|
|:--|
|调整为通过`Lua`脚本合并执行`Redis`的多条更新指令|

**2021-10-18 Release** 
|CheckCodeHelper v1.0.3|
|:--|
|增加`EffectiveTimeDisplayedInContent`以调整验证码有效期在发送内容中的展示方式，`ComplexHelper`已支持该枚举，`ComplexSetting.EffectiveTimeDisplayed`默认设置为`Seconds`，即在所有的发送内容中以秒对应的数字进行展示，设置为`Auto`时如果有效时间为整360秒或以上且可被360整除，则展示为对应的小时数，有效时间为整60秒或以上且可被60整除，则展示为对应的分钟数<br>
`SendResult.NotSupprot`修正拼写错误为`SendResult.NotSupport`；`VerificationResult.VerificationFailed`简化为`Failed`。注意对于`CheckCodeHelper`这是**破坏性调整**，会造成升级后相关判断产生错误|
|**CheckCodeHelper.Sender.EMail v1.0.3**|
|移除不必要的`Subject Func`，同步调整`TextFormat`及`Subject`到`EMailMimeMessageSetting`，注意对于`EMail`部分这是一个**破坏性调整**  |
|**CheckCodeHelper.Sender.AlibabaSms v1.0.0**|
|基于阿里模板短信的`ICodeSender`实现|

**2021-07-20 Release** 
|CheckCodeHelper v1.0.2|
|:--|
|增加`PeriodLimit.Interval`以支持限制发送检验码的时间间隔|
|**CheckCodeHelper.Sender.Sms v1.0.1**|
|升级依赖的`RestSharp`版本至`106.12.0`以解决潜在的安全问题|

**2021-07-17 Release**
|CheckCodeHelper v1.0.1|
|:--|
|增加`ComplexHelper`以统一在单个应用中校验码的发送与验证入口，支持按需调用指定的`ICodeSender`|
|**CheckCodeHelper.Sender.EMail v1.0.2**|
|增加`Subject Func`以支持`ComplexHelper`|

**2021-07-15 Release**
|CheckCodeHelper v1.0.0|
|:--|
|主体功能|
|**CheckCodeHelper.Sender.Sms v1.0.0**|
|基于非模板短信的`ICodeSender`实现|
|**CheckCodeHelper.Sender.EMail v1.0.1**|
|基于EMail的`ICodeSender`实现|
|**CheckCodeHelper.Storage.Redis v1.0.0**|
|基于`Redis`的`ICodeStorage`实现|
|**CheckCodeHelper.Storage.Memory v1.0.0**|
|基于`MemoryCache`的`ICodeStorage`实现|

