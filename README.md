# CheckCodeHelper
适用于发送验证码及校验验证码的场景（比如找回密码功能）  
支持周期内限制最大发送次数，支持单次发送后最大校验错误次数  
存储方案默认提供了`Redis`和`MemoryCache`实现，你也可以自己实现`ICodeStorage`来支持其它存储方案。

[![NuGet version (CheckCodeHelper)](https://img.shields.io/nuget/v/CheckCodeHelper.svg?style=flat-square)](https://www.nuget.org/packages/CheckCodeHelper/)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://mit-license.org/)

## .NET版本支持
支持以下版本：`.NET45`(Sms短信部分为`.NET452`)、`.NET Standard 2.0`

## 如何使用
你可以在此处查看使用例子 https://github.com/fdstar/CheckCodeHelper/blob/master/src/CheckCodeHelper.Samples/Program.cs

## Release History
**2021-07-17**
- Release v1.0.1 增加`ComplexHelper`以统一在单个应用中校验码的发送与验证入口，支持按需调用指定的`ICodeSender`

**2021-07-15**
- Release v1.0.0

