# 无敌lib
为方便 C# 调用酷 Q [HTTP API](https://github.com/richardchien/coolq-http-api) 插件而开发的lib。

- Named by [int100](https://github.com/1004121460)

## 如何使用
克隆本项目，然后在您的项目中添加对本项目的引用。

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
由于历史原因，Token 使用静态属性设置，不同的客户端实例只能设置相同的 API Token。未来将会修改。
```C#
HttpApiClient.AccessToken = "this-is-your-token";
```
#### Secret
可以为每个监听实例设置不同的 Secret。
```C#
listener.SetSecret("this-is-your-secret");
```
设置后，每次收到上报都会验证上报数据的哈希。如果验证失败，将忽略此次上报。

### 与现有代码共同使用
WudiLib 支持将收到的上报数据转发到另一处，相当于有两个上报地址，使得以前的代码可以继续运行，降低迁移成本。
```C#
listener.ForwardTo = "http://[::1]:10202"; // 转发路径，监听到的事件会被转发到此处。
```
只要原来的代码监听 http://[::1]:10202，就可以不用任何修改继续运行，就像直接上报到 http://[::1]:10202 一样。

*注意：转发功能暂不支持设置 Secret。也不支持直接通过上报数据进行响应（请访问 API 以响应上报）。*

## Nuget 包
Nuget 包[在此](https://www.nuget.org/packages/Sisters.WudiLib/)。

注意：虽然本项目已经几近完成，但依然有少量细节需要完善。目前建议您直接添加项目引用，这样您就可以按自己的需要修改代码。此外，等到本项目功能添加得差不多的时候，可能将迎来一个不兼容的新版本，以提高使用体验，暂定版本号为 0.1.x（目前是 0.0.x）。0.0.x 在此之后应该会继续维护一段时间。