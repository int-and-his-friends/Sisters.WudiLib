<div lang="zh-CN">

# 无敌lib
为方便 C# 调用酷 Q [HTTP API](https://github.com/richardchien/coolq-http-api) 插件而开发的lib。

[![#](https://img.shields.io/nuget/v/Sisters.WudiLib.svg)](https://www.nuget.org/packages/Sisters.WudiLib/)
[![#](https://img.shields.io/nuget/v/Sisters.WudiLib.WebSocket.svg)](https://www.nuget.org/packages/Sisters.WudiLib.WebSocket/)

- Named by [int100](https://github.com/1004121460)

## 如何使用
### 安装酷 Q 和 CQHTTP
安装过程不赘述。
### 在你的 C#（或 .NET）项目中引入
dotnet 命令行
```
dotnet add package Sisters.WudiLib
```
或者使用 Visual Studio 添加。
nuget 程序包在 https://www.nuget.org/packages/Sisters.WudiLib 。

## 用法
### 发送消息、调用 API
``` C#
var httpApi = new HttpApiClient();
httpApi.ApiAddress = "http://127.0.0.1:5700/";
var privateResponse = await httpApi.SendPrivateMessageAsync(12345678, "hello");
```
### 监听事件
``` C#
var listener = new ApiPostListener();
listener.ApiClient = httpApi; // 上面所示客户端的实例，将作为参数传给事件处理器，便于进行各种操作。
listener.PostAddress = "http://127.0.0.1:8080/";
listener.StartListen();
listener.AnonymousMessageEvent += ApiPostListener.RepeatAsync; // 复读匿名消息。
```

### 发送图片、语音等消息
可以通过 `SendingMessage` 的静态方法构造各种类型的消息，然后通过 `+` 连接。
```C#
// 网络图片消息。
var netImage = SendingMessage.NetImage("https://your.image.url/file.jpg");
// 文本消息。
var textMessage = new SendingMessage("这是一条文本消息。");
// 混合排版消息。
var mixedMessage = netImage + textMessage;
```

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
### 正向 WebSocket
您可以在此找到 WebSocket 通信的 nuget 包：[Sisters.WudiLib.WebSocket](https://www.nuget.org/packages/Sisters.WudiLib.WebSocket/)。

### 特性
使用 WebSocket 监听上报，与 HTTP 方式并没有很大不同。需要注意的一点是，第一次调用 `StartListen` 方法必须成功连接，否则会引发异常。此后如果连接断开，会自动尝试重连。你可以通过传入 `CancellationToken` 进行终止。具体请参见示例。

目前支持通过 WebSocket 监听上报和访问 API。请求类事件将通过 API 响应，必须设置了 `ApiClient` 属性才可以。

### 示例
此示例包含了简单的事件监听和处理，并对
```C#
var cqWebSocketEvent = new CqHttpWebSocketEvent(
    "wss://your-ws-address/event",
    "your-access-token"); // 创建 WebSocket 事件监听客户端。
var httpApiClient = new CqHttpWebSocketApiClient(
    "wss://your-ws-address/event",
    "your-access-token"); // 创建 HTTP 通信客户端。
cqWebSocketEvent.ApiClient = httpApiClient;

// 订阅事件。
cqWebSocketEvent.MessageEvent += (api, e) =>
{
    Console.WriteLine(e.Content.Text);
};
cqWebSocketEvent.FriendRequestEvent += (api, e) =>
{
    return true;
};
cqWebSocketEvent.GroupInviteEvent += (api, e) =>
{
    return true;
}; // 可以通过 return 的方式响应请求，与使用 HTTP 时没有差别。

// 每秒打印 WebSocket 状态。
Task.Run(async () =>
{
    while (true)
    {
        await Task.Delay(1000);
        Console.WriteLine("Available: {0}, Listening {1}", cqWebSocketEvent.IsAvailable, cqWebSocketEvent.IsListening);
    }
});

// 连接前等待 3 秒观察状态。
Task.Delay(TimeSpan.FromSeconds(3)).Wait();

// 连接（开始监听上报）。
var cancellationTokenSource = new CancellationTokenSource();
cqWebSocketEvent.StartListen(cancellationTokenSource.Token); // 首次连接必须成功。

// 按下回车会在 2 秒后断开，再过 3 秒使用新的 CancellationTokenSource 重连。
// 您可以先断开网络，观察自动重连，再继续执行后面的代码。
Console.ReadLine();
cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(2));
Task.Delay(TimeSpan.FromSeconds(5)).Wait();
cancellationTokenSource.Dispose();
cancellationTokenSource = new CancellationTokenSource();
cqWebSocketEvent.StartListen(cancellationTokenSource.Token);
Task.Delay(-1).Wait();
```

### 其他通信方式
从 0.0.4 版本开始，WudiLib 的某些基础方法已使用 `virtual` 标记，也就是说，您可以继承并重载相关的类，以实现其他通信方式（如反向 WebSocket）。

### 与现有代码共同使用
WudiLib 支持将收到的上报数据转发到另一处，相当于有两个上报地址，使得以前的代码可以继续运行，降低迁移成本。
```C#
listener.ForwardTo = "http://[::1]:10202"; // 转发路径，监听到的事件会被转发到此处。
```
只要原来的代码监听 http://[::1]:10202，就可以不用任何修改继续运行，就像直接上报到 http://[::1]:10202 一样。

*注意：转发功能将传递头部中的 `X-Signature` 字段，也就是相当于使用相同的 Secret。不支持直接通过上报数据进行响应（请访问 API 以响应上报）。*

## 开发现状
WudiLib 实现了访问 CQHTTP 所有主要接口，监听除心跳外的事件。WudiLib 使用简单、运行稳定、效率尚可，只是部分类名有待商榷，使用略有不便。请见下面的“小建议”。

尽管我没有精力把 WudiLib 制作成完整的框架，但我会追踪 CQHTTP 的更新，在 API 变化时尽快更新，并尽管满足用户的需求。

### 小建议
由于 `Sisters.WudiLib.Message` 和 `Sisters.WudiLib.Posts.Message` 类的类名相同，使用起来有诸多不便，建议您在每个**新**代码文件开头添加下列 `using`：
```C#
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;
```
这样，就可以用 `MessageContext` 表示收到的消息上报，用 `Message` 表示要发送的消息了。

## 帮助
如果您需要帮助，请联系 QQ：962549599，注明“WudiLib”和您的称呼。

</div>