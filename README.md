<div lang="zh-CN">

# 无敌lib
为方便 C# 调用酷 Q [HTTP API](https://github.com/richardchien/coolq-http-api) 插件而开发的lib。

[![#](https://img.shields.io/nuget/v/Sisters.WudiLib.svg)](https://www.nuget.org/packages/Sisters.WudiLib/)
[![#](https://img.shields.io/nuget/v/Sisters.WudiLib.WebSocket.svg)](https://www.nuget.org/packages/Sisters.WudiLib.WebSocket/)

[查看文档](https://wudilib.b11p.com/)

- Named by [int100](https://github.com/1004121460)

## 如何使用
### 发送消息、调用 API、监听事件
见：[快速上手](https://wudilib.b11p.com/zhinan/kuaisushangshou.html)。

### 发送图片、语音等消息
见：[进阶 WudiLib](https://wudilib.b11p.com/zhinan/jinjie-wudilib.html)

### Token 和 Secret
#### Token
可以为每个客户端设置不同的 AccessToken。
```C#
httpApi.AccessToken = "this-is-your-token";
```

#### Secret
可以为每个监听实例设置不同的 Secret。
```C#
listener.SetSecret("this-is-your-secret");
```
设置后，每次收到上报都会验证上报数据的哈希。如果验证失败，将忽略此次上报。

## WebSocket 和其他通信方式
- [正向 WebSocket](https://wudilib.b11p.com/tongxinfangshi/zhengxiang-websocket.html)
- [反向 WebSocket](https://wudilib.b11p.com/tongxinfangshi/fanxiang-websocket.html)
- [扩展其他通信方式](https://wudilib.b11p.com/kuozhan/tongxinfangshi.html)

<!-- ### 与现有代码共同使用
WudiLib 支持将收到的上报数据转发到另一处，相当于有两个上报地址，使得以前的代码可以继续运行，降低迁移成本。
```C#
listener.ForwardTo = "http://[::1]:10202"; // 转发路径，监听到的事件会被转发到此处。
```
只要原来的代码监听 http://[::1]:10202，就可以不用任何修改继续运行，就像直接上报到 http://[::1]:10202 一样。

*注意：转发功能将传递头部中的 `X-Signature` 字段，也就是相当于使用相同的 Secret。不支持直接通过上报数据进行响应（请访问 API 以响应上报）。* -->

## 开发现状
积极开发中。可以在[路线图](https://wudilib.b11p.com/luxiantu.html)中查看当前开发的目标。也欢迎提出任何 Issue 和 Pull Request。

### 小建议
由于 `Sisters.WudiLib.Message` 和 `Sisters.WudiLib.Posts.Message` 类的类名相同，使用起来有诸多不便，建议您在每个**新**代码文件开头添加下列 `using`：
```C#
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;
```
这样，就可以用 `MessageContext` 表示收到的消息上报，用 `Message` 表示要发送的消息了。

## 帮助
如果您需要帮助，请联系 QQ：962549599，注明“WudiLib”和您的称呼。更欢迎直接提出 Issue。

</div>