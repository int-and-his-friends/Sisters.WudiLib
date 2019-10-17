# 无敌lib
为方便 C# 调用酷 Q [HTTP API](https://github.com/richardchien/coolq-http-api) 插件而开发的lib。

- Named by [int100](https://github.com/1004121460)

## 如何使用
dotnet 命令行
```
dotnet add package Sisters.WudiLib
```
或者使用 Visual Studio 添加。
nuget 程序包在 https://www.nuget.org/packages/Sisters.WudiLib 。

## 帮助
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
### 通过 WebSocket 监听上报
您可以在此找到 WebSocket 通信的 nuget 包：[Sisters.WudiLib.WebSocket](https://www.nuget.org/packages/Sisters.WudiLib.WebSocket/)。

### 特性
使用 WebSocket 监听上报，与 HTTP 方式并没有很大不同。需要注意的一点是，第一次调用 `StartListen` 方法必须成功连接，否则会引发异常。此后如果连接断开，会自动尝试重连。你可以通过传入 `CancellationToken` 进行终止。具体请参见示例。

目前仅支持通过 WebSocket 监听上报（暂不支持通过 WebSocket 进行 API 访问，请使用 HTTP）。请求类事件将通过 API 响应，必须设置了 `ApiClient` 属性才可以。请与 HTTP 方式结合使用。

### 示例
此示例包含了简单的事件监听和处理，并对
```C#
var cqWebSocketEvent = new CqHttpWebSocketEvent(
    "wss://your-ws-address/event",
    "your-access-token"); // 创建 WebSocket 事件监听客户端。
var httpApiClient = new HttpApiClient(); // 创建 HTTP 通信客户端。
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
从 0.0.4 版本开始，WudiLib 的某些基础方法已使用 `virtual` 标记，也就是说，您可以继承并重载相关的类，以实现 WebSocket 监听上报和访问 API。

### 与现有代码共同使用
WudiLib 支持将收到的上报数据转发到另一处，相当于有两个上报地址，使得以前的代码可以继续运行，降低迁移成本。
```C#
listener.ForwardTo = "http://[::1]:10202"; // 转发路径，监听到的事件会被转发到此处。
```
只要原来的代码监听 http://[::1]:10202，就可以不用任何修改继续运行，就像直接上报到 http://[::1]:10202 一样。

*注意：转发功能将传递头部中的 `X-Signature` 字段，也就是相当于使用相同的 Secret。不支持直接通过上报数据进行响应（请访问 API 以响应上报）。*

## 小建议
由于 `Sisters.WudiLib.Message` 和 `Sisters.WudiLib.Posts.Message` 类的类名相同（设计问题，以后会改），使用起来有诸多不便，建议您在每个**新**代码文件开头添加下列 `using`：
```C#
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;
```
这样，就可以用 `MessageContext` 表示收到的消息上报，用 `Message` 表示要发送的消息了。

## 帮助
如果您需要帮助，请联系 QQ：962549599，注明“WudiLib”。

## 更新日志（部分）
### 0.0.2
- 优化了多次获取 `ReceivedMessage.Sections` 属性时的性能。
- 增加 `ReceivedMessage.TryGetPlainText(out string text)` 方法
- 现在转发时将保持头部中的 `X-Signature` 不变。

### 0.0.3
#### API
- 将 API 访问网络的方法移到 `HttpApiClient` 类，并提取 `CallRawAsync()` 方法，方便重载。
- 增加 `HttpApiClient.CallAsync()` 方法，方便调用本类库没提供的 API。可以使用继承或扩展方法的方式封装。
#### `Section`
- 为 `Section` 增加 `Raw` 属性，方便查看其字符串形式。
- 为 `Section` 增加几个构造方法，方便构造自己的消息段。
- 修复 `Section` 在参数顺序不同时 `GetHashCode()` 返回不同结果的 bug。现在参数顺序不分先后。*潜在可能影响现有代码行为，但目前未找到真正有影响的情况。*
#### `Message` 及其子类
- `Message` 类的构造方法改为 `protected`，`Serializing` 属性改为 `protected internal abstract`（原来是 `internal abstract`）。所以你可以继承并实现自己的 Message。
- 修改了现有的 `Message` 子类 `Serializing` 属性的修饰符，以配合上一项。
- 为 `SendingMessage` 增加 `Sections` 属性，返回类型为 `IReadOnlyList<Section>`。
#### `Message` 及其子类不兼容的更改 **(注意：不向后兼容)**
- 修改了 `SectionMesssage` 的大部分内容。现在 `SectionMessage` 的构造方法均被限制，也就是说，无法从外部继承。可能在以后重新开放。
#### 框架
- 支持 .NET Framework 4.5。

### 0.0.4
- 修改 `HttpApiClient` 类中的部分虚方法，以便更好地支持 WebSocket。主要更改是把 `url` 参数改为了 `action`。*如果您重载过 `CallRawAsync` 方法，您需要修改代码以确保代码正常运行。*
- `GroupMemberInfo` 中的属性被补齐。
- 增加禁言匿名成员、禁言发送人（通过 `MessageSource` 自动识别是不是匿名成员）、全体禁言的 API。
- `ApiPostListener` 的 `StartListen` 方法、`PostAddress` 和 `IsListening` 属性改为 `virtual`，方便实现 WebSocket。
- `ApiPostListener.RepeatAsync` 方法使用 try-catch 包围，以免发生异常导致程序崩溃。
- 增加 `SenderInfo` 类。`GroupMessage` 类中增加 `Sender` 字段（需要 CoolQ HTTP API 插件版本 >= 4.7.0）。

### 0.0.4.2
- 修复使用 `array` 上报类型时，访问 `Content` 出错的问题。
- 细节特性更新。

### 0.0.5
- 新增 `get_status`、`get_group_list` 和 `get_friend_list` API。
- 新增群禁言事件。
- 处理了加群、好友等请求时，请求消息含有 CQ 码的情况。
- 增加了 `Endpoint` 和 `MessageSource` 的 `ToString()` 方法重载。
- `HttpClient` 改为单例模式。
- 减少了转发时的重新编码损耗。