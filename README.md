# CheckCodeHelper
适用于发送验证码及校验验证码的场景（比如找回密码功能）  
支持周期内限制最大发送次数，支持单次发送后最大校验错误次数  
存储方案默认提供了`Redis`实现，你也可以自己实现`ICodeStorage`来支持其它存储方案。

## .NET版本支持
支持以下版本：`.NET461`、`.NET Standard 2.0`

## 如何使用
你可以在此处查看使用例子 https://github.com/fdstar/CheckCodeHelper/blob/master/src/CheckCodeHelper.Samples/Program.cs
